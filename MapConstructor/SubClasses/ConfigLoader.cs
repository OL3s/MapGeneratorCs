using System.Text.Json;

namespace MapGeneratorCs
{
    internal static class ConfigLoader
    {
        private static readonly bool enableDebugLogging = true;
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

        public static MapConstructor.SpawnFactors LoadSpawnFactor()
        {
            if (enableDebugLogging)
                Console.WriteLine("Loading spawn factors from config/configSpawnFactor.json");
            return LoadConfig<MapConstructor.SpawnFactors>("config/configSpawnFactor.json");
        }

        public static MapConstructor.ObjectGenerator.GenerateObjectWeights[] LoadGenObjectProperties()
        {
            if (enableDebugLogging)
                Console.WriteLine("Loading generate object properties from config/configGenerateObjects.json");
            return LoadConfig<MapConstructor.ObjectGenerator.GenerateObjectWeights[]>("config/configGenerateObjects.json");
        }

    }
}