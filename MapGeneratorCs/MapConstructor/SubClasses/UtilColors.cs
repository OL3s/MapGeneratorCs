using SixLabors.ImageSharp.PixelFormats;

namespace MapGeneratorCs
{
    internal static class TileColorUtils
    {
        public static Rgba32 GetColorForTileType(MapConstructor.TileSpawnType type)
        {
            return type switch
            {
                MapConstructor.TileSpawnType.Empty => new Rgba32(0, 0, 0, 255),
                MapConstructor.TileSpawnType.Default => new Rgba32(128, 128, 128, 255),
                MapConstructor.TileSpawnType.StartGenerator => new Rgba32(0, 255, 0, 255),
                MapConstructor.TileSpawnType.EndGenerator => new Rgba32(255, 0, 0, 255),
                MapConstructor.TileSpawnType.TreasureGenerator => new Rgba32(255, 255, 0, 255),
                MapConstructor.TileSpawnType.EnemyGenerator => new Rgba32(128, 0, 128, 255),
                MapConstructor.TileSpawnType.LandmarkGenerator => new Rgba32(0, 0, 255, 255),
                MapConstructor.TileSpawnType.BossGenerator => new Rgba32(139, 0, 0, 255),
                MapConstructor.TileSpawnType.QuestGenerator => new Rgba32(255, 165, 0, 255),
                MapConstructor.TileSpawnType.TrapGenerator => new Rgba32(105, 105, 105, 255),
                MapConstructor.TileSpawnType.DefaultGenerator => new Rgba32(192, 192, 192, 255),
                MapConstructor.TileSpawnType.EmptyGenerator => new Rgba32(64, 64, 64, 255),
                MapConstructor.TileSpawnType.TreasureObject => new Rgba32(255, 215, 0, 255),
                MapConstructor.TileSpawnType.EnemyCollector => new Rgba32(75, 0, 130, 255),
                MapConstructor.TileSpawnType.EnemyObject => new Rgba32(148, 0, 211, 255),
                MapConstructor.TileSpawnType.BossObject => new Rgba32(220, 20, 60, 255),
                MapConstructor.TileSpawnType.MainBossObject => new Rgba32(178, 34, 34, 255),
                MapConstructor.TileSpawnType.LandmarkObject => new Rgba32(70, 130, 180, 255),
                MapConstructor.TileSpawnType.QuestObject => new Rgba32(255, 140, 0, 255),
                MapConstructor.TileSpawnType.PropObject => new Rgba32(139, 69, 19, 255),
                MapConstructor.TileSpawnType.TrapObject => new Rgba32(105, 105, 105, 255),
                MapConstructor.TileSpawnType.StartObject => new Rgba32(34, 139, 34, 255),
                MapConstructor.TileSpawnType.EndObject => new Rgba32(178, 34, 34, 255),
                MapConstructor.TileSpawnType.WaterTile => new Rgba32(0, 191, 255, 255),
                MapConstructor.TileSpawnType.LavaTile => new Rgba32(255, 69, 0, 255),
                _ => new Rgba32(255, 0, 0, 255),
            };
        }
    }
}