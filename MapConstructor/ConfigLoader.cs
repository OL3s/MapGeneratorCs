using System.Text.Json;
using static MapGeneratorCs.MapConstructor.ObjectGenerator;
using static MapGeneratorCs.MapConstructor;

namespace MapGeneratorCs
{
    internal static class ConfigLoader
    {
        private static readonly bool enableDebugLogging = false;
        public static T LoadConfig<T>(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Config file not found: {filePath}");

            string json = File.ReadAllText(filePath);
            var result = JsonSerializer.Deserialize<T>(json);
            if (result == null)
                throw new InvalidOperationException($"Failed to deserialize config file: {filePath}");
            if (enableDebugLogging)
                Console.WriteLine($"Loaded config from {filePath}");
            return result;
        }

        public static SpawnWeights LoadSpawnWeights()
        {
            if (enableDebugLogging)
                Console.WriteLine("Loading spawn weights from config/configSpawnWeights.json");

            return LoadConfig<SpawnWeights>("config/configSpawnWeights.json");
        }

        public static GenerateObjectWeights[] LoadObjectWeights()
        {
            if (enableDebugLogging)
                Console.WriteLine("Loading object weights from config/configObjectsWeights.json");
            return LoadConfig<GenerateObjectWeights[]>("config/configObjectsWeights.json");
        }

        public static MapWeights LoadMapWeights()
        {
            if (enableDebugLogging)
                Console.WriteLine("Loading map weights from config/configMapWeights.json");
            return LoadConfig<MapWeights>("config/configMapWeights.json");
        }

        // create config files with default values if they do not exist
        public static void InitConfigFiles(string folderPath, bool overwriteExisting = false)
        {

            Console.WriteLine($"{(overwriteExisting ? "Overwriting" : "Checking")} config files...");

            // Ensure config directory exists
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // spawn weights
            string spawnWeightsPath = Path.Combine(folderPath, "configSpawnWeights.json");
            if (!File.Exists(spawnWeightsPath))
                ConfigConstructor.CreateSpawnWeights(spawnWeightsPath);

            // generate object weights
            string genObjectsPath = Path.Combine(folderPath, "configObjectsWeights.json");
            if (!File.Exists(genObjectsPath) || overwriteExisting)
                ConfigConstructor.CreateObjectsWeights(genObjectsPath);

            string mapWeightsPath = Path.Combine(folderPath, "configMapWeights.json");
            if (!File.Exists(mapWeightsPath) || overwriteExisting)
                ConfigConstructor.CreateMapWeights(mapWeightsPath);

        }

        private static class ConfigConstructor
        {
            public static void CreateObjectsWeights(string path)
            {

                // Create default generate object properties
                int count = JSONIndexFromTileSpawnTypeGenerator(null);
                var defaultGenObjects = new GenerateObjectWeights[count];

                // DefaultGenerator
                defaultGenObjects[JSONIndexFromTileSpawnTypeGenerator(TileSpawnType.DefaultGenerator)] = new GenerateObjectWeights
                {
                    Description = GetDescFromTileSpawnTypeGenerator(TileSpawnType.DefaultGenerator),
                    EmptyWeight = 2,
                    PropWeight = 1
                };

                // EnemyGenerator
                defaultGenObjects[JSONIndexFromTileSpawnTypeGenerator(TileSpawnType.EnemyGenerator)] = new GenerateObjectWeights
                {
                    Description = GetDescFromTileSpawnTypeGenerator(TileSpawnType.EnemyGenerator),
                    EmptyWeight = 4,
                    PropWeight = 1,
                };

                // LandmarkGenerator
                defaultGenObjects[JSONIndexFromTileSpawnTypeGenerator(TileSpawnType.LandmarkGenerator)] = new GenerateObjectWeights
                {
                    Description = GetDescFromTileSpawnTypeGenerator(TileSpawnType.LandmarkGenerator),
                    EmptyWeight = 5,
                    PropWeight = 1,
                };

                // TreasureGenerator
                defaultGenObjects[JSONIndexFromTileSpawnTypeGenerator(TileSpawnType.TreasureGenerator)] = new GenerateObjectWeights
                {
                    Description = GetDescFromTileSpawnTypeGenerator(TileSpawnType.TreasureGenerator),
                    EmptyWeight = 5,
                    PropWeight = 2,
                    TreasureWeight = 1
                };

                // TrapGenerator
                defaultGenObjects[JSONIndexFromTileSpawnTypeGenerator(TileSpawnType.TrapGenerator)] = new GenerateObjectWeights
                {
                    Description = GetDescFromTileSpawnTypeGenerator(TileSpawnType.TrapGenerator),
                    EmptyWeight = 1,
                    PropWeight = 1,
                    TrapWeight = 10
                };

                // EmptyGenerator
                defaultGenObjects[JSONIndexFromTileSpawnTypeGenerator(TileSpawnType.EmptyGenerator)] = new GenerateObjectWeights
                {
                    Description = GetDescFromTileSpawnTypeGenerator(TileSpawnType.EmptyGenerator),
                    EmptyWeight = 12,
                    PropWeight = 1,
                };

                // BossGenerator
                defaultGenObjects[JSONIndexFromTileSpawnTypeGenerator(TileSpawnType.BossGenerator)] = new GenerateObjectWeights
                {
                    Description = GetDescFromTileSpawnTypeGenerator(TileSpawnType.BossGenerator),
                    EmptyWeight = 0,
                    PropWeight = 0,
                };

                // QuestGenerator
                defaultGenObjects[JSONIndexFromTileSpawnTypeGenerator(TileSpawnType.QuestGenerator)] = new GenerateObjectWeights
                {
                    Description = GetDescFromTileSpawnTypeGenerator(TileSpawnType.QuestGenerator),
                    EmptyWeight = 5,
                    PropWeight = 1,
                };

                // StartGenerator
                defaultGenObjects[JSONIndexFromTileSpawnTypeGenerator(TileSpawnType.StartGenerator)] = new GenerateObjectWeights
                {
                    Description = GetDescFromTileSpawnTypeGenerator(TileSpawnType.StartGenerator),
                    EmptyWeight = 0,
                    PropWeight = 0,
                };

                // Default for others
                defaultGenObjects[count - 1] = new GenerateObjectWeights
                {
                    Description = GetDescFromTileSpawnTypeGenerator(null),
                    EmptyWeight = 2,
                    PropWeight = 1,
                };

                string json = JsonSerializer.Serialize(defaultGenObjects, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
                Console.WriteLine($"Created default generate object properties config at {path}");
            }

            private static string GetDescFromTileSpawnTypeGenerator(TileSpawnType? type)
            {
                if (type == null)
                    return "Default";
                return $"{Enum.GetName(typeof(TileSpawnType), type)}";
            }

            public static void CreateSpawnWeights(string path)
            {
                // Create default spawn weights
                var defaultSpawnWeights = new SpawnWeights
                {
                    enemy = 1,
                    landmark = 1,
                    treasure = 1,
                    _default = 1,
                    empty = 1,
                    trap = 1,
                    props = 1
                };

                // Serialize and save to file
                string json = JsonSerializer.Serialize(defaultSpawnWeights, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);

                Console.WriteLine($"Created default spawn factor config at {path}");
            }

            // Map TileSpawnType to JSON index for generator types; null for default or/and count
            public static int JSONIndexFromTileSpawnTypeGenerator(TileSpawnType? type)
            {
                return type switch
                {
                    TileSpawnType.DefaultGenerator => 0,
                    TileSpawnType.EnemyGenerator => 1,
                    TileSpawnType.LandmarkGenerator => 2,
                    TileSpawnType.TreasureGenerator => 3,
                    TileSpawnType.TrapGenerator => 4,
                    TileSpawnType.EmptyGenerator => 5,
                    TileSpawnType.BossGenerator => 6,
                    TileSpawnType.QuestGenerator => 7,
                    TileSpawnType.StartGenerator => 8,
                    TileSpawnType.EndGenerator => 9,
                    _ => 10,
                };
            }

            internal static void CreateMapWeights(string mapWeightsPath)
            {
                // Create default map weights
                var defaultMapWeights = new MapWeights
                {
                    Length = 100,
                    Seed = null,
                    CollisionRadius = 2,
                    Thickness = 1,
                    FlagBoss = true,
                    FlagQuest = true
                };

                // Serialize and save to file
                string json = JsonSerializer.Serialize(defaultMapWeights, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(mapWeightsPath, json);

                Console.WriteLine($"Created default map weights config at {mapWeightsPath}");
            }
        }
    }
}