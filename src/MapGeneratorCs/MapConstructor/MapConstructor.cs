
using MapGeneratorCs.Types;
using MapGeneratorCs.Utils;
namespace MapGeneratorCs;

public class MapConstructor
{
    internal Random random;

    public SpawnWeights spawnWeights;
    public MapConfig mapConfig;
    internal int padding = 1;
    public NodeContainerData NodeContainer { get; set; } = new NodeContainerData();

    public MapConstructor()
    {

        ConfigLoader.InitConfigFiles();
        random = new Random();
    }

    public void GenerateMap()
    {
        // Start details
        Console.WriteLine("Starting Map Generation...");
        this.mapConfig = ConfigLoader.LoadMapConfig();
        this.spawnWeights = ConfigLoader.LoadSpawnWeights();
        this.random = (mapConfig.Seed == null) ? this.random : new Random((int)mapConfig.Seed);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Init generation
        BasicBuilder.GenerateDefaultAndFlaggedNotes(this);
        BasicBuilder.FillDefaultNodesWithTypeNodes(this);
        ObjectBuilder.GenerateObjectNodes(this);

        // Finalize details
        stopwatch.Stop();
        Console.WriteLine("\n=== Generated Map Statistics ===\n"
            + $"Floor nodes           {NodeContainer.NodesFloor.Count,6} stk\n"
            + $"Raw floor nodes       {NodeContainer.NodesFloorRaw.Count,6} stk\n"
            + $"Parent nodes          {NodeContainer.NodesGenerate.Count,6} stk\n"
            + $"Object nodes          {NodeContainer.NodesObjects.Count,6} stk\n"
            + $"Generation time       {stopwatch.ElapsedMilliseconds,6} .ms\n");

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

}