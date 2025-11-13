using MapGeneratorCs.Generator.Utils;
using MapGeneratorCs.Types;
using MapGeneratorCs.Generator.Types;
using MapGeneratorCs.Generator.Utils.Builders;
namespace MapGeneratorCs;

public class MapConstructor
{
    internal Random random;
    public SpawnWeights spawnWeights;
    public MapConfig mapConfig;
    internal int padding = 1;
    public NodeContainerData NodeContainer { get; set; } = new NodeContainerData();

    public MapConstructor(bool overwriteExisting = false, SpawnWeights? spawnWeights = null, MapConfig? mapConfig = null)
    {
        ConfigLoader.InitConfigFiles(overwriteExisting: overwriteExisting, spawnWeights: spawnWeights, mapConfig: mapConfig);
        this.spawnWeights = spawnWeights ?? ConfigLoader.LoadSpawnWeights();
        this.mapConfig = mapConfig ?? ConfigLoader.LoadMapConfig();
        random = new Random();
    }

    public MapConstructor(string jsonFileLoadPath)
    {
        LoadMapFromJson(jsonFileLoadPath);
        random = new Random();
    }

    public void GenerateMap()
    {
        // Start details
        Console.WriteLine("Starting Map Generation...");
        this.random = (mapConfig.Seed == null) ? this.random : new Random((int)mapConfig.Seed);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Init generation
        GeneratorBuilder.GenerateDefaultAndFlaggedNotes(this);
        GeneratorBuilder.FillDefaultNodesWithTypeNodes(this);
        ObjectBuilder.GenerateObjectNodes(this);

        // Finalize details
        stopwatch.Stop();
        Console.WriteLine("\n=== Generated Map Statistics ===\n"
            + $"Floor nodes           {NodeContainer.NodesFloor.Count,6} stk\n"
            + $"Raw floor nodes       {NodeContainer.NodesFloorRaw.Count,6} stk\n"
            + $"Parent nodes          {NodeContainer.NodesGenerate.Count,6} stk\n"
            + $"Object nodes          {NodeContainer.NodesObjects.Count,6} stk\n"
            + $"Generation time       {stopwatch.ElapsedMilliseconds,6} .ms\n"
            + $""
            );

    }


    public void ResetConfigToDefaults()
    {
        ConfigLoader.InitConfigFiles(overwriteExisting: true);
    }
    public void SaveMapAsImage(string filePath = "export/map_output.png")
    {
        var grid = IntMapBuilder.CreateGridFromMap(this);
        IntMapBuilder.SaveGridToImage(grid, filePath, includeGenerateNodes: false);
    }
    public void SaveMapAsJson(string filePath = "export/map_output.json")
    {
        JsonMapBuilder.SaveMapAsJson(this, filePath);
    }
    public void SaveAll() 
    {
        var grid = IntMapBuilder.CreateGridFromMap(this);
        IntMapBuilder.SaveGridToImage(grid, "export/map_output.png", includeGenerateNodes: false);
        JsonMapBuilder.SaveMapAsJson(this, "export/map_output.json");
    }
    public void LoadMapFromJson(string filePath = "export/map_output.json")
    {
        var loaded = JsonMapBuilder.LoadMapFromJson(filePath);
        // Copy loaded data into this instance
        this.NodeContainer = loaded.NodeContainer;
        this.mapConfig = loaded.mapConfig;
        this.spawnWeights = loaded.spawnWeights;
        this.random = loaded.random;
        this.padding = loaded.padding;
    }
    public void ClearAllData()
    {
        ConfigLoader.DeleteAll();
    }

    public List<Vect2D> FindClosestObjectNodesOfTypeByAirDistance
    (       Vect2D from,
            Dictionary<Vect2D, TileSpawnType> objectNodes,
            TileSpawnType? searchType = null,
            int? maxSearchDistance = null,
            int? maxObjectCount = null
    )
    {
        var foundNodes = MapUtils.FindClosestObjectNodesOfTypeByAirDistance(
            from,
            objectNodes,
            searchType,
            maxSearchDistance,
            maxObjectCount
        );
        return foundNodes;
    }

    // Vect2D getters for flagged nodes
    public Vect2D StartPosition => NodeContainer.NodesObjects.FirstOrDefault(kv => kv.Value == TileSpawnType.StartObject).Key;
    public Vect2D EndPosition => NodeContainer.NodesObjects.FirstOrDefault(kv => kv.Value == TileSpawnType.EndObject).Key;
    public Vect2D BossPosition => NodeContainer.NodesObjects.FirstOrDefault(kv => kv.Value == TileSpawnType.BossObject).Key;
    public Vect2D MainBossPosition => NodeContainer.NodesObjects.FirstOrDefault(kv => kv.Value == TileSpawnType.MainBossObject).Key;
    public Vect2D QuestPosition => NodeContainer.NodesObjects.FirstOrDefault(kv => kv.Value == TileSpawnType.QuestObject).Key;

}