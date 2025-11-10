using System.Text.Json;

namespace MapGeneratorCs.Utils;
public static class JsonMapBuilder
{
    public static void SaveMapAsJson(MapConstructor map, string filePath)
    {
        Console.WriteLine($"Saving map to JSON {filePath}...");
        var options = new JsonSerializerOptions { WriteIndented = true };
        options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());

        // Convert Dictionary<Vect2D, TileSpawnType> to Dictionary<string, TileSpawnType> for serialization
        // And HashSet<Vect2D> to List<string>
        var serializableNodeContainer = new serializableNodeContainer
        {
            NodesFloor = new List<string>(),
            NodesFloorRaw = new List<string>(),
            NodesGenerate = new Dictionary<string, TileSpawnType>(),
            NodesObjects = new Dictionary<string, TileSpawnType>()
        };

        // convert vect2d to string
        foreach (var v in map.NodeContainer.NodesFloor)
            serializableNodeContainer.NodesFloor.Add($"{v.x},{v.y}");

        foreach (var v in map.NodeContainer.NodesFloorRaw)
            serializableNodeContainer.NodesFloorRaw.Add($"{v.x},{v.y}");

        foreach (var kvp in map.NodeContainer.NodesGenerate)
            serializableNodeContainer.NodesGenerate.Add($"{kvp.Key.x},{kvp.Key.y}", kvp.Value);

        foreach (var kvp in map.NodeContainer.NodesObjects)
            serializableNodeContainer.NodesObjects.Add($"{kvp.Key.x},{kvp.Key.y}", kvp.Value);

        // Serialize the entire NodeContainer
        string json = JsonSerializer.Serialize(serializableNodeContainer, options);
        File.WriteAllText(filePath, json);
        Console.WriteLine("Map saved to JSON successfully.");
    }

    public static MapConstructor LoadMapFromJson(string filePath)
    {
        Console.WriteLine($"Loading map from JSON {filePath}...");
        var map = new MapConstructor();
        var options = new JsonSerializerOptions();
        options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());

        string json = File.ReadAllText(filePath);
        serializableNodeContainer? loaded = JsonSerializer.Deserialize<serializableNodeContainer>(json, options);

        if (loaded == null)
            throw new InvalidDataException("Failed to deserialize JSON map data.");

        // Convert back to original structures
        foreach (var node in loaded.NodesFloor)
        {
            var parts = node.Split(',');
            map.NodeContainer.NodesFloor.Add(new Vect2D(int.Parse(parts[0]), int.Parse(parts[1])));
        }

        foreach (var node in loaded.NodesFloorRaw)
        {
            var parts = node.Split(',');
            map.NodeContainer.NodesFloorRaw.Add(new Vect2D(int.Parse(parts[0]), int.Parse(parts[1])));
        }

        foreach (var kvp in loaded.NodesGenerate)
        {
            var keyParts = kvp.Key.Split(',');
            map.NodeContainer.NodesGenerate.Add(new Vect2D(int.Parse(keyParts[0]), int.Parse(keyParts[1])), kvp.Value);
        }

        foreach (var kvp in loaded.NodesObjects)
        {
            var keyParts = kvp.Key.Split(',');
            map.NodeContainer.NodesObjects.Add(new Vect2D(int.Parse(keyParts[0]), int.Parse(keyParts[1])), kvp.Value);
        }

        Console.WriteLine("Map loaded from JSON successfully.");
        return map;
    }

    private class serializableNodeContainer
    {
        public List<string> NodesFloor { get; set; } = new();
        public List<string> NodesFloorRaw { get; set; } = new();
        public Dictionary<string, TileSpawnType> NodesGenerate { get; set; } = new();
        public Dictionary<string, TileSpawnType> NodesObjects { get; set; } = new();
    }
}