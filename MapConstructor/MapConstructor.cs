using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MapGeneratorCs
{
    // === Main Map Constructor Class (core) ===
    partial class MapConstructor
    {
        private readonly bool ENABLE_DETAILED_LOGGING;
        private Random random;
        public struct SpawnWeights
        {
            public int enemy { get; set; }
            public int landmark { get; set; }
            public int treasure { get; set; }
            public int _default { get; set; }
            public int empty { get; set; }
            public int trap { get; set; }
            public int props { get; set; }
        }
        private SpawnWeights spawnWeights;
        public struct MapWeights
        {

            public int Length { get; set; } = 1000;
            public int CollisionRadius { get; set; } = 5;
            public int Thickness { get; set; } = 1;
            public int? Seed { get; set; } = null;
            public bool FlagBoss { get; set; } = true;
            public bool FlagQuest { get; set; } = true;
            public MapWeights() { }
        }
        private MapWeights mapWeights;
        private int[,]? TileMap2D;
        private int padding = 1;

        public class NodeContainerData
        {
            [JsonInclude]
            public HashSet<Vect2D> NodesFloor { get; set; } = new();
            [JsonInclude]
            public HashSet<Vect2D> NodesFloorRaw { get; set; } = new();
            [JsonInclude]
            public Dictionary<Vect2D, TileSpawnType> NodesGenerate { get; set; } = new();
            [JsonInclude]
            public Dictionary<Vect2D, TileSpawnType> NodesObjects { get; set; } = new();
        }

        public NodeContainerData NodeContainer;
        public struct Vect2D
        {
            public int x;
            public int y;
            public Vect2D(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        public MapConstructor(bool enableDetailedLogging = true)
        {

            ConfigLoader.InitConfigFiles("config", "export", false);
            this.mapWeights = ConfigLoader.LoadMapWeights();
            this.spawnWeights = ConfigLoader.LoadSpawnWeights();
            this.random = new Random(mapWeights.Seed ?? new Random().Next());
            this.ENABLE_DETAILED_LOGGING = enableDetailedLogging;

            NodeContainer = new NodeContainerData
            {
                NodesFloor = new HashSet<Vect2D>(),
                NodesFloorRaw = new HashSet<Vect2D>(),
                NodesGenerate = new Dictionary<Vect2D, TileSpawnType>(),
                NodesObjects = new Dictionary<Vect2D, TileSpawnType>()
            };
        }

        public void GenerateMap()
        {
            // Start details
            Console.WriteLine("Starting Map Generation...\n");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Init generation
            GenerationBasics.GenerateDefaultAndFlaggedNotes(this);
            GenerationBasics.FillDefaultNodesWithTypeNodes(this);
            ObjectGenerator.GenerateObjectDictionary(this);

            // Finalize details
            stopwatch.Stop();
            Console.WriteLine("\n=== Generated Map Statistics ===\n"
                + $"Floor nodes           {NodeContainer.NodesFloor.Count,6} stk\n"
                + $"Raw floor nodes       {NodeContainer.NodesFloorRaw.Count,6} stk\n"
                + $"Parent nodes          {NodeContainer.NodesGenerate.Count,6} stk\n"
                + $"Object nodes          {NodeContainer.NodesObjects.Count,6} stk\n"
                + $"Generation time       {stopwatch.ElapsedMilliseconds,6} .ms\n");

        }

        public void SaveMapAsImage()
        {
            IntMapBuilder.BuildFromNodes(this);
            IntMapBuilder.SaveToImage(this, "export/" + "map_output.png");
        }
        public void SaveMapAsJson()
        {
            IntMapBuilder.BuildFromNodes(this);
            JsonMapBuilder.SaveMapAsJson(this, "export/" + "map_output.json");
        }

        public void LoadMapFromJson(string filePath = "export/map_output.json")
        {
            JsonMapBuilder.LoadMapFromJson(this, filePath);
        }

        // Expose internals to helper classes
        internal Random RNG => random;
        internal int Length => mapWeights.Length;
        internal int Thickness => mapWeights.Thickness;
        internal int CollisionRadius => mapWeights.CollisionRadius;
        internal int Padding => padding;
        internal (bool isBoss, bool isQuest) SpawnTypeFlags => (mapWeights.FlagBoss, mapWeights.FlagQuest);
        internal SpawnWeights SpawnWeightValues => spawnWeights;
        internal int[,]? Grid => TileMap2D;
        internal bool Verbose => ENABLE_DETAILED_LOGGING;
    }
}