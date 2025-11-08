using System;
using System.IO;
using System.Text.Json;

namespace MapGeneratorCs
{
    internal static class ConfigLoader
    {
        private static readonly bool enableLogging = true;
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

        public static MapConstructor.SpawnFactors LoadSpawnFactor()
        {
            return LoadConfig<MapConstructor.SpawnFactors>("config/configSpawnFactor.json");
        }

        public static MapConstructor.ObjectGenerator.GenerateObjectWeights[] LoadGenObjectProperties()
        {
            return LoadConfig<MapConstructor.ObjectGenerator.GenerateObjectWeights[]>("config/configGenerateObjects.json");
        }

    }
}