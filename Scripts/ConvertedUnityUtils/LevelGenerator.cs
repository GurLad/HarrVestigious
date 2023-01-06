using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Numerics;

public class LevelGenerator : Node
{
    // General data
    [Export]
    public string WallsCSVPath;
    [Export]
    public string EntitiesJSONPath;
    [Export]
    public int TileSize = 16;
    [Export]
    public int PhysicalSize = 1;
    // Terrain
    [Export]
    public PackedScene FloorScene;
    // Units
    [Export]
    public PackedScene OrcScene;
    private LevelData levelData;
    private int[,] walls;

    public override void _Ready()
    {
        base._Ready();
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
        levelData = LevelData.Interpret(entitiesJSON.ToString(), TileSize); // TEMP - fix later, tired
        // Generate walls
        walls = ImportWalls(wallsCSV.ToString(), levelData.Width, levelData.Height); // TEMP - fix later, tired
        for (int x = 0; x < levelData.Width; x++)
        {
            for (int y = 0; y < levelData.Height; y++)
            {
                switch (walls[x, y])
                {
                    case 0: // Floor
                        Spatial newFloor = FloorScene.Instance<Spatial>();
                        newFloor.Translate(new Vector2Int(x, y).To3D() * PhysicalSize);
                        AddChild(newFloor);
                        break;
                    case 1: // Wall
                        //GameObject newWall = Instantiate(Wall, WallHolder);
                        //newWall.transform.position += new Vector2Int(x, y).To3D() * PhysicalSize;
                        //newWall.SetActive(true);
                        break;
                    case 2: // Void
                        break;
                    case 4: // Door
                        break;
                    case 3: // Pathfinding blocker - aka floor
                    default:
                        break;
                }
            }
        }
        // Generate objects
        foreach (List<Entity> entities in levelData.entities.Values)
        {
            foreach (Entity entity in entities)
            {
                Unit entityObject = null;
                Vector2Int pos = new Vector2Int(entity.x / TileSize, entity.y / TileSize);
                switch (entity.id)
                {
                    case "Stairs":
                    case "Orc":
                        entityObject = OrcScene.Instance<Unit>();
                        break;
                    default:
                        throw new System.Exception("What");
                }
                entityObject.Translate(pos.To3D() * PhysicalSize);
                AddChild(entityObject);
            }
        }
        // Init pathfinder
        Pathfinder.SetMap(walls, new Vector2Int(levelData.Width, levelData.Height));
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

        public EntityField() { }
        public EntityField(System.Int64 data) { intData = data; }

        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(cx, cy);
        }

        public static implicit operator EntityField(System.Int64 i) =>
            new EntityField(i);
        public static implicit operator System.Int64(EntityField ef) =>
            ef.intData;
    }
}
