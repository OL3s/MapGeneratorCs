using System;
using System.Collections.Generic;

namespace MapGeneratorCs
{
    // === Main Map Constructor Class (core) ===
    partial class MapConstructor
    {
        private readonly bool ENABLE_DETAILED_LOGGING;
        private (int x, int y) currentPosition;
        public int Seed { get; private set; }
        private Random random;
        private int length;
        private int thickness;
        private int collisionRadius;
        private (
            int enemy,
            int landmark,
            int treasure,
            int _default,
            int empty,
            int trap
        ) spawnFactor;
        private (bool isBoss, bool isQuest) spawnTypeFlags;
        private int[,]? TileMap2D;
        private int padding = 1;

        internal struct NodeContainerData
        {
            public HashSet<(int x, int y)> NodesFloor;
            public HashSet<(int x, int y)> NodesFloorRaw;
            public Dictionary<(int x, int y), TileSpawnType> NodesGenerate;
            public Dictionary<(int x, int y), TileSpawnType> NodesObjects;
        };

        internal NodeContainerData NodeContainer;

        public MapConstructor(
            int length, int thickness, int collisionRadius, int seed,
            (int enemyFactor, int landmarkFactor, int treasureFactor, int emptyFactor, int defaultFactor,
                int trapFactor, bool isBoss, bool isQuest) spawnFactors, bool enableDetailedLogging = true)
        {
            this.length = length;
            this.collisionRadius = collisionRadius;
            this.spawnFactor = (
                spawnFactors.enemyFactor,
                spawnFactors.landmarkFactor,
                spawnFactors.treasureFactor,
                spawnFactors.defaultFactor,
                spawnFactors.emptyFactor,
                spawnFactors.trapFactor
            );
            this.Seed = seed;
            this.random = new Random(seed);
            this.thickness = thickness;
            this.spawnTypeFlags = (spawnFactors.isBoss, spawnFactors.isQuest);
            this.ENABLE_DETAILED_LOGGING = enableDetailedLogging;

            NodeContainer = new NodeContainerData
            {
                NodesFloor = new HashSet<(int x, int y)>(),
                NodesFloorRaw = new HashSet<(int x, int y)>(),
                NodesGenerate = new Dictionary<(int x, int y), TileSpawnType>(),
                NodesObjects = new Dictionary<(int x, int y), TileSpawnType>()
            };
        }

        public void GenerateMap()
        {
            Console.WriteLine("Starting Map Generation...\n");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            GenerationBasics.GenerateDefaultAndFlaggedNotes(this);
            GenerationBasics.FillDefaultNodesWithTypeNodes(this);
            ObjectGenerator.GenerateObjectDictionary(this);
            stopwatch.Stop();
            Console.WriteLine("\n=== Generated Map Statistics ===\n"
                + $"Floor nodes           {NodeContainer.NodesFloor.Count,6} stk\n"
                + $"Raw floor nodes       {NodeContainer.NodesFloorRaw.Count,6} stk\n"
                + $"Parent nodes          {NodeContainer.NodesGenerate.Count,6} stk\n"
                + $"Object nodes          {NodeContainer.NodesObjects.Count,6} stk\n"
                + $"Generation time       {stopwatch.ElapsedMilliseconds,6} .ms\n");

        }

        public void SaveMapAsImage(string filePath)
        {
            try
            {
                IntMapBuilder.BuildFromNodes(this);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error building IntMap2D: {ex.Message}");
                return;
            }
            IntMapBuilder.SaveToImage(this, filePath);
        }
        

        // Expose internals to helper classes
        internal Random RNG => random;
        internal int Length => length;
        internal int Thickness => thickness;
        internal int CollisionRadius => collisionRadius;
        internal int Padding => padding;
        internal (bool isBoss, bool isQuest) SpawnTypeFlags => spawnTypeFlags;
        internal (int enemy, int landmark, int treasure, int _default, int empty, int trap) SpawnFactor => spawnFactor;
        internal int[,]? Grid => TileMap2D;
        internal ref (int x, int y) CurrentPos => ref currentPosition;
        internal bool Verbose => ENABLE_DETAILED_LOGGING;
    }
}