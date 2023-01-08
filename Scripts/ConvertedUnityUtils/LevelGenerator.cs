using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class LevelGenerator : Node
{
    public static readonly int PHYSICAL_SIZE = 2;
    private enum State { Idle, FadeOut, FadeIn }
    // General data
    [Export]
    public string LevelsPath;
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
    [Export]
    public PackedScene LavaScene;
    // Units
    [Export]
    public PackedScene TorchScene;
    [Export]
    public PackedScene LadderScene;
    [Export]
    public PackedScene OrcScene;
    [Export]
    public PackedScene GoblinScene;
    [Export]
    public PackedScene SkeletonScene;
    [Export]
    public PackedScene ImpScene;
    [Export]
    public PackedScene GolemScene;
    private LevelData levelData;
    private int[,] walls;
    private Spatial objectsHolder;
    private Camera camera;
    private TurnFlowController turnFlowController;
    private FloorMarker floorMarker;
    private PlayerUIController playerUIController;
    private TutorialController tutorialController;
    // Transition stuff
    private State state;
    private Control blackScreen;
    private Timer transitionTimer;
    private int currentLevel;
    private Action midTransition;
    private Action postTransition;

    public override void _Ready()
    {
        base._Ready();
        objectsHolder = GetNode<Spatial>("Objects/LevelObjects");
        camera = GetNode<Camera>("Objects/Camera");
        turnFlowController = GetNode<TurnFlowController>("TurnFlowController");
        floorMarker = GetNode<FloorMarker>("FloorMarker");
        playerUIController = GetNode<PlayerUIController>("PlayerUIController");
        tutorialController = GetNode<TutorialController>("TutorialController");
        blackScreen = GetNode<Control>("GameUI/BlackScreen");
        transitionTimer = GetNode<Timer>("TransitionTimer");
        // Generate first level
        GenerateLevel(6);
        transitionTimer.Start();
        postTransition = BeginLevel;
        state = State.FadeIn;
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        switch (state)
        {
            case State.Idle:
                break;
            case State.FadeOut:
                blackScreen.Modulate = new Color(blackScreen.Modulate, transitionTimer.Percent());
                if (transitionTimer.TimeLeft <= 0)
                {
                    state = State.FadeIn;
                    midTransition?.Invoke();
                    transitionTimer.Start();
                }
                break;
            case State.FadeIn:
                blackScreen.Modulate = new Color(blackScreen.Modulate, 1 - transitionTimer.Percent());
                if (transitionTimer.TimeLeft <= 0)
                {
                    state = State.Idle;
                    blackScreen.MouseFilter = Control.MouseFilterEnum.Ignore;
                    postTransition?.Invoke();
                }
                break;
            default:
                break;
        }
    }

    public void Win()
    {
        Transition(() => GenerateLevel(currentLevel + 1), BeginLevel);
    }

    public void Lose()
    {
        Transition(() => GenerateLevel(currentLevel), BeginLevel);
    }

    public void _OnGameOver(bool won)
    {
        if (won)
        {
            Win();
        }
        else
        {
            Lose();
        }
    }

    public void _OnTutorialContinue()
    {
        BeginLevel();
    }

    private void GenerateLevel(int number)
    {
        currentLevel = number;
        // Clear previous level
        turnFlowController.RemoveAllUnits();
        foreach (Node child in objectsHolder.GetChildren())
        {
            if (!child.IsQueuedForDeletion())
            {
                child.QueueFree();
            }
        }
        camera.Translation = Vector3.Zero;
        // Read CSV
        var file = new File();
        file.Open(LevelsPath + number + WallsCSVPath, File.ModeFlags.Read);
        string wallsCSV = file.GetAsText();
        file.Close();
        // Read JSON
        file = new File();
        file.Open(LevelsPath + number + EntitiesJSONPath, File.ModeFlags.Read);
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
                Floor newFloor;
                switch (walls[x, y])
                {
                    case 0: // Floor
                        newFloor = FloorScene.Instance<Floor>();
                        newFloor.Translate(new Vector2Int(x, y).To3D());
                        objectsHolder.AddChild(newFloor);
                        floorMarker.AddFloor(x, y, newFloor);
                        break;
                    case 1: // Outer Wall
                        Spatial newOuterWall = OuterWallScene.Instance<Spatial>();
                        newOuterWall.Translate(new Vector2Int(x, y).To3D());
                        objectsHolder.AddChild(newOuterWall);
                        // Generate a floor below
                        newFloor = FloorScene.Instance<Floor>();
                        newFloor.Translate(new Vector2Int(x, y).To3D());
                        objectsHolder.AddChild(newFloor);
                        break;
                    case 2: // Wall
                        Spatial newWall = WallScene.Instance<Spatial>();
                        newWall.Translate(new Vector2Int(x, y).To3D());
                        objectsHolder.AddChild(newWall);
                        // Generate a floor below
                        newFloor = FloorScene.Instance<Floor>();
                        newFloor.Translate(new Vector2Int(x, y).To3D());
                        objectsHolder.AddChild(newFloor);
                        break;
                    case 3: // Lava
                        Floor newLava = LavaScene.Instance<Floor>();
                        newLava.Translate(new Vector2Int(x, y).To3D());
                        objectsHolder.AddChild(newLava);
                        floorMarker.AddFloor(x, y, newLava);
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
                    case "Ladder":
                        entityObject = LadderScene.Instance<Spatial>();
                        floorMarker.SetWinFloor(pos.x, pos.y);
                        break;
                    case "Orc":
                        entityObject = unitObject = OrcScene.Instance<Unit>();
                        CreateUnit(unitObject, pos, entity);
                        break;
                    case "Goblin":
                        entityObject = unitObject = GoblinScene.Instance<Unit>();
                        CreateUnit(unitObject, pos, entity);
                        unitObject.PatrolPoint = new Vector2Int(entity.customFields["Patrol"].cx, entity.customFields["Patrol"].cy);
                        break;
                    case "Skeleton":
                        entityObject = unitObject = SkeletonScene.Instance<Unit>();
                        CreateUnit(unitObject, pos, entity);
                        break;
                    case "Imp":
                        entityObject = unitObject = ImpScene.Instance<Unit>();
                        CreateUnit(unitObject, pos, entity);
                        break;
                    case "Golem":
                        entityObject = unitObject = GolemScene.Instance<Unit>();
                        CreateUnit(unitObject, pos, entity);
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
        // Init tutorial
        tutorialController.NewLevel(currentLevel);
    }

    private void CreateUnit(Unit unitObject, Vector2Int pos, Entity entity)
    {
        unitObject.Init();
        unitObject.Pos = pos;
        unitObject.HasVest = entity.customFields["HasVest"].boolData;
        unitObject.FloorMarker = floorMarker;
        unitObject.TurnFlowController = turnFlowController;
        unitObject.PlayerUIController = playerUIController;
        turnFlowController.AddUnit(unitObject);
        Pathfinder.PlaceObject(pos);
    }

    private void BeginLevel()
    {
        if (tutorialController.NextTutorial())
        {
            // Init turn flow
            turnFlowController.Begin();
        }
    }

    private void Transition(Action midTransition, Action postTransition)
    {
        blackScreen.MouseFilter = Control.MouseFilterEnum.Stop;
        this.midTransition = midTransition;
        this.postTransition = postTransition;
        state = State.FadeOut;
        transitionTimer.Start();
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
