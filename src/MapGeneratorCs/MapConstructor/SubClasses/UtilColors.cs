using SixLabors.ImageSharp.PixelFormats;

namespace MapGeneratorCs.Utils;

public static class TileColorUtils
{
    public static Rgba32 GetColorForTileType(TileSpawnType type)
    {
        return type switch
        {
            TileSpawnType.Empty => new Rgba32(0, 0, 0, 255),
            TileSpawnType.Default => new Rgba32(128, 128, 128, 255),
            TileSpawnType.StartGenerator => new Rgba32(0, 255, 0, 255),
            TileSpawnType.EndGenerator => new Rgba32(255, 0, 0, 255),
            TileSpawnType.TreasureGenerator => new Rgba32(255, 255, 0, 255),
            TileSpawnType.EnemyGenerator => new Rgba32(128, 0, 128, 255),
            TileSpawnType.LandmarkGenerator => new Rgba32(0, 0, 255, 255),
            TileSpawnType.BossGenerator => new Rgba32(139, 0, 0, 255),
            TileSpawnType.QuestGenerator => new Rgba32(255, 165, 0, 255),
            TileSpawnType.TrapGenerator => new Rgba32(105, 105, 105, 255),
            TileSpawnType.DefaultGenerator => new Rgba32(192, 192, 192, 255),
            TileSpawnType.EmptyGenerator => new Rgba32(64, 64, 64, 255),
            TileSpawnType.TreasureObject => new Rgba32(255, 215, 0, 255),
            TileSpawnType.EnemyCollector => new Rgba32(75, 0, 130, 255),
            TileSpawnType.EnemyObject => new Rgba32(148, 0, 211, 255),
            TileSpawnType.BossObject => new Rgba32(220, 20, 60, 255),
            TileSpawnType.MainBossObject => new Rgba32(178, 34, 34, 255),
            TileSpawnType.LandmarkObject => new Rgba32(70, 130, 180, 255),
            TileSpawnType.QuestObject => new Rgba32(255, 140, 0, 255),
            TileSpawnType.PropObject => new Rgba32(139, 69, 19, 255),
            TileSpawnType.TrapObject => new Rgba32(105, 105, 105, 255),
            TileSpawnType.StartObject => new Rgba32(34, 139, 34, 255),
            TileSpawnType.EndObject => new Rgba32(178, 34, 34, 255),
            TileSpawnType.WaterTile => new Rgba32(0, 191, 255, 255),
            TileSpawnType.LavaTile => new Rgba32(255, 69, 0, 255),
            _ => new Rgba32(255, 0, 0, 255),
        };
    }
}
