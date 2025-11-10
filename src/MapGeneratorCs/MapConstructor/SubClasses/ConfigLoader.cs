using System.Text.Json;
using MapGeneratorCs.Types;

namespace MapGeneratorCs.Utils;
public static class ConfigLoader
{
    public static T LoadConfig<T>(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Config file not found: {filePath}");

        string json = File.ReadAllText(filePath);
        var result = JsonSerializer.Deserialize<T>(json);
        if (result == null)
            throw new InvalidOperationException($"Failed to deserialize config file: {filePath}");
        return result;
    }

    public static SpawnWeights LoadSpawnWeights()
    {

        return LoadConfig<SpawnWeights>("config/configSpawnWeights.json");
    }

    public static Dictionary<int, GenerateObjectWeights> LoadObjectWeights()
    {
        return LoadConfig<Dictionary<int, GenerateObjectWeights>>("config/configObjectsWeights.json");
    }

    public static MapConfig LoadMapConfig()
    {
        return LoadConfig<MapConfig>("config/configMapConfig.json");
    }

    // create config files with default values if they do not exist
    public static void InitConfigFiles(string folderPathConfig = "config", string folderPathExport = "export", bool overwriteExisting = false)
    {

        Console.WriteLine($"{(overwriteExisting ? "Overwriting" : "Checking")} config files...");

        // Ensure config directory exists
        if (!Directory.Exists(folderPathConfig))
            Directory.CreateDirectory(folderPathConfig);

        // Ensure export directory exists
        if (!Directory.Exists(folderPathExport))
            Directory.CreateDirectory(folderPathExport);

        // spawn weights
        string spawnWeightsPath = Path.Combine(folderPathConfig, "configSpawnWeights.json");
        if (!File.Exists(spawnWeightsPath))
            ConfigConstructor.CreateSpawnWeights(spawnWeightsPath);

        // generate object weights
        string genObjectsPath = Path.Combine(folderPathConfig, "configObjectsWeights.json");
        if (!File.Exists(genObjectsPath) || overwriteExisting)
            ConfigConstructor.CreateObjectsWeights(genObjectsPath);

        string mapWeightsPath = Path.Combine(folderPathConfig, "configMapConfig.json");
        if (!File.Exists(mapWeightsPath) || overwriteExisting)
            ConfigConstructor.CreateMapWeights(mapWeightsPath);
    }

    public static void DeleteAll()
    {
        Console.WriteLine("Deleting all default generated files and all its contents...");
        if (Directory.Exists("config"))
            Directory.Delete("config", true);
        if (Directory.Exists("export"))
            Directory.Delete("export", true);
        
    }

    private static class ConfigConstructor
    {
        public static void CreateObjectsWeights(string path)
        {

            // Create default generate object properties
            var defaultGenObjects = new Dictionary<int, GenerateObjectWeights>();

            // DefaultGenerator
            defaultGenObjects.Add((int)TileSpawnType.DefaultGenerator, new GenerateObjectWeights
                { EmptyWeight = 2, PropWeight = 1 });

            // EnemyGenerator
            defaultGenObjects.Add((int)TileSpawnType.EnemyGenerator, new GenerateObjectWeights
                { EmptyWeight = 4, PropWeight = 1 });

            // LandmarkGenerator
            defaultGenObjects.Add((int)TileSpawnType.LandmarkGenerator, new GenerateObjectWeights
                { EmptyWeight = 5, PropWeight = 1, });

            // TreasureGenerator
            defaultGenObjects.Add((int)TileSpawnType.TreasureGenerator, new GenerateObjectWeights
                { EmptyWeight = 5, PropWeight = 2, TreasureWeight = 1 });

            // TrapGenerator
            defaultGenObjects.Add((int)TileSpawnType.TrapGenerator, new GenerateObjectWeights
                { EmptyWeight = 1, PropWeight = 1, TrapWeight = 10 });

            // EmptyGenerator
            defaultGenObjects.Add((int)TileSpawnType.EmptyGenerator, new GenerateObjectWeights
                { EmptyWeight = 12, PropWeight = 1, });

            // BossGenerator
            defaultGenObjects.Add((int)TileSpawnType.BossGenerator, new GenerateObjectWeights
                { EmptyWeight = 0, PropWeight = 0 });

            // QuestGenerator
            defaultGenObjects.Add((int)TileSpawnType.QuestGenerator, new GenerateObjectWeights
                { EmptyWeight = 5, PropWeight = 1, });

            // StartGenerator
            defaultGenObjects.Add((int)TileSpawnType.StartGenerator, new GenerateObjectWeights
                { EmptyWeight = 0, PropWeight = 0 });

            string json = JsonSerializer.Serialize(defaultGenObjects, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
            Console.WriteLine($"Created default generate object properties config at {path}");
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

        internal static void CreateMapWeights(string mapWeightsPath)
        {
            // Create default map weights
            var defaultMapWeights = new MapConfig
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