using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class LevelGenerator : Node
{
    public static readonly int PHYSICAL_SIZE = 2;
    // General data
    [Export]
    public string WallsCSVPath;
    [Export]
    public string EntitiesJSONPath;
    [Export]
    public int TileSize = 16;
    // Terrain
    [Export]
    public PackedScene FloorScene;
    [Export]
    public PackedScene OuterWallScene;
    [Export]
    public PackedScene WallScene;
    // Units
    [Export]
    public PackedScene TorchScene;
    [Export]
    public PackedScene OrcScene;
    private LevelData levelData;
    private int[,] walls;
    private Spatial objectsHolder;
    private Camera camera;
    private TurnFlowController turnFlowController;
    private FloorMarker floorMarker;
    private PlayerUIController playerUIController;

    public override void _Ready()
    {
        base._Ready();
        objectsHolder = GetNode<Spatial>("Objects");
        camera = GetNode<Camera>("Objects/Camera");
        turnFlowController = GetNode<TurnFlowController>("TurnFlowController");
        floorMarker = GetNode<FloorMarker>("FloorMarker");
        playerUIController = GetNode<PlayerUIController>("PlayerUIController");
        // Read CSV
        var file = new File();
        file.Open("res://" + WallsCSVPath, File.ModeFlags.Read);
        string wallsCSV = file.GetAsText();
        file.Close();
        // Read JSON
        file = new File();
        file.Open("res://" + EntitiesJSONPath, File.ModeFlags.Read);
        string entitiesJSON = file.GetAsText();
        file.Close();
        levelData = LevelData.Interpret(entitiesJSON, TileSize);
        // Generate walls
        walls = ImportWalls(wallsCSV, levelData.Width, levelData.Height);
        floorMarker.NewLevel(new Vector2Int(levelData.Width, levelData.Height), turnFlowController);
        for (int x = 0; x < levelData.Width; x++)
        {
            for (int y = 0; y < levelData.Height; y++)
            {
                switch (walls[x, y])
                {
                    case 0: // Floor
                        Floor newFloor = FloorScene.Instance<Floor>();
                        newFloor.Translate(new Vector2Int(x, y).To3D());
                        objectsHolder.AddChild(newFloor);
                        floorMarker.AddFloor(x, y, newFloor);
                        break;
                    case 1: // Outer Wall
                        Spatial newOuterWall = OuterWallScene.Instance<Spatial>();
                        newOuterWall.Translate(new Vector2Int(x, y).To3D());
                        objectsHolder.AddChild(newOuterWall);
                        break;
                    case 2: // Wall
                        Spatial newWall = WallScene.Instance<Spatial>();
                        newWall.Translate(new Vector2Int(x, y).To3D());
                        objectsHolder.AddChild(newWall);
                        break;
                    case 3: // Void
                        break;
                    case 4: // Door
                        break;
                    case 5: // Pathfinding blocker - aka floor
                    default:
                        break;
                }
            }
        }
        // Init pathfinder
        Pathfinder.SetMap(walls, new Vector2Int(levelData.Width, levelData.Height));
        // Generate objects
        foreach (List<Entity> entities in levelData.entities.Values)
        {
            foreach (Entity entity in entities)
            {
                Spatial entityObject = null;
                Unit unitObject;
                Vector2Int pos = new Vector2Int(entity.x / TileSize, entity.y / TileSize);
                switch (entity.id)
                {
                    case "Torch":
                        entityObject = TorchScene.Instance<Spatial>();
                        Vector3 offset = entity.customFields["Direction"].ToVector2Int().To3D() - pos.To3D();
                        offset = offset.Normalized() * 1.166f;
                        entityObject.Translate(offset);
                        break;
                    case "Orc":
                        entityObject = unitObject = OrcScene.Instance<Unit>();
                        unitObject.Pos = pos;
                        unitObject.HasVest = entity.customFields["HasVest"].boolData;
                        unitObject.FloorMarker = floorMarker;
                        unitObject.TurnFlowController = turnFlowController;
                        unitObject.PlayerUIController = playerUIController;
                        turnFlowController.AddUnit(unitObject);
                        Pathfinder.PlaceObject(pos);
                        break;
                    default:
                        throw new System.Exception("What");
                }
                entityObject.Translate(pos.To3D());
                objectsHolder.AddChild(entityObject);
            }
        }
        // Camrea
        camera.Translate(new Vector3(-(levelData.Width / 2.0f - 0.5f), -(levelData.Height / 2.0f - 0.5f), (levelData.Width - 2) / 2.0f) * PHYSICAL_SIZE);
        // Init turn flow
        turnFlowController.Begin();
    }

    private int SafeGetWall(int x, int y)
    {
        if (x < 0 || y < 0 || x >= levelData.Width || y >= levelData.Height)
        {
            return 0;
        }
        return walls[x, y];
    }

    private int[,] ImportWalls(string csv, int width, int height)
    {
        int[,] result = new int[width, height];
        string[] rows = csv.Replace("\r", "").Split('\n');
        for (int y = 0; y < rows.Length - 1; y++) // Ends with newline
        {
            string row = rows[y][rows[y].Length - 1] == ',' ? rows[y].Substring(0, rows[y].Length - 1) : rows[y];
            string[] columns = row.Split(',');
            for (int x = 0; x < columns.Length; x++)
            {
                //Debug.Log("(" + x + ", " + y + "): " + columns[x]);
                result[x, y] = int.Parse(columns[x]) - 1;
            }
        }
        return result;
    }

    [System.Serializable]
    private class LevelData
    {
        public Dictionary<string, List<Entity>> entities;
        [Newtonsoft.Json.JsonProperty]
        private int width;
        public int Width => width / tileSize;
        [Newtonsoft.Json.JsonProperty]
        private int height;
        public int Height => height / tileSize;
        private int tileSize;

        public static LevelData Interpret(string json, int tileSize)
        {
            LevelData levelData = new LevelData();
            levelData = JsonConvert.DeserializeObject<LevelData>(json);
            levelData.tileSize = tileSize;
            //JsonUtility.FromJsonOverwrite(json, this);
            //Debug.Log(JsonConvert.SerializeObject(levelData));
            return levelData;
        }
    }

    [System.Serializable]
    private class Entity
    {
        public string id;
        public int x;
        public int y;
        public int width;
        public int height;
        public Dictionary<string, EntityField> customFields;
    }

    [System.Serializable]
    private class EntityField
    {
        public int cx;
        public int cy;
        public System.Int64 intData;
        public bool boolData;

        public EntityField() { }
        public EntityField(System.Int64 data) { intData = data; }
        public EntityField(bool data) { boolData = data; }

        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(cx, cy);
        }

        public static implicit operator EntityField(System.Int64 i) =>
            new EntityField(i);
        public static implicit operator System.Int64(EntityField ef) =>
            ef.intData;
        public static implicit operator EntityField(bool b) =>
            new EntityField(b);
        public static implicit operator bool(EntityField ef) =>
            ef.boolData;
    }
}
