using MapGeneratorCs.Generator.Types;
using MapGeneratorCs.Types;

namespace MapGeneratorCs.ExportKit;

public static class ExportKit
{
    // NEW: config record for tuning padded spawn
    public record PaddedConfig(
        int Radius = 6,                // sampling radius
        double Exponent = 2.0,         // curve steepness (higher -> only deep interior)
        double MaxChance = .8         // cap on final probability
    );

    // Old signature kept for backward compatibility
    public static ExportedTilessetBackground GenerateDefaultBackground(NodeContainerData nodeContainerData)
        => GenerateDefaultBackground(nodeContainerData, new PaddedConfig());

    // New tunable version
    public static ExportedTilessetBackground GenerateDefaultBackground(
        NodeContainerData nodeContainerData,
        PaddedConfig cfg)
    {
        int MinInteriorFloorCount = (int)Math.Round(cfg.Radius * cfg.Radius * 0.8);
        var background = new ExportedTilessetBackground();

        foreach (var pos in nodeContainerData.NodesFloor)
        {
            background[pos] = ExportTileBackground.Default;

            // openness stats
            (int floorCount, int totalCount) = ComputeOpennessCounts(pos, nodeContainerData, cfg.Radius);

            // HARD GATE: too close to wall => skip padding
            if (floorCount < MinInteriorFloorCount)
            {
                // still check top wall
                if (IsTopWall(pos, nodeContainerData))
                {
                    var above = new Vect2D(pos.x, pos.y - 1);
                    background[above] = ExportTileBackground.TopWall;
                }
                continue;
            }

            // normalized openness after gate (0..1)
            int effectiveRange = totalCount - MinInteriorFloorCount;
            double normalized = effectiveRange <= 0
                ? 0
                : (double)(floorCount - MinInteriorFloorCount) / effectiveRange;

            // curve
            double probability = Math.Min(cfg.MaxChance, Math.Pow(normalized, cfg.Exponent));

            if (Random.Shared.NextDouble() < probability)
                background[pos] = ExportTileBackground.Padded;

            if (IsTopWall(pos, nodeContainerData))
            {
                var above = new Vect2D(pos.x, pos.y - 1);
                background[above] = ExportTileBackground.TopWall;
            }
        }
        return background;
    }

    private static bool IsTopWall(Vect2D pos, NodeContainerData nodeContainerData)
    {
        var abovePos = new Vect2D(pos.x, pos.y - 1);
        return nodeContainerData.NodesFloor.Contains(pos) &&
               !nodeContainerData.NodesFloor.Contains(abovePos);
    }

    // Returns raw counts so we can apply a threshold
    private static (int floorCount, int totalCount) ComputeOpennessCounts(
        Vect2D pos,
        NodeContainerData nodeContainerData,
        int radius)
    {
        int floorCount = 0;
        int totalCount = 0;

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx * dx + dy * dy > radius * radius) continue;
                totalCount++;
                var checkPos = new Vect2D(pos.x + dx, pos.y + dy);
                if (nodeContainerData.NodesFloor.Contains(checkPos))
                    floorCount++;
            }
        }
        return (floorCount, totalCount);
    }
}

public enum ExportTileBackground
{
    None,
    Default,
    Padded,
    TopWall,
}

public class ExportedTilessetBackground : Dictionary<Vect2D, ExportTileBackground>
{
    
}