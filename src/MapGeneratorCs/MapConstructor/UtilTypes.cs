namespace MapGeneratorCs.Types;
public struct SpawnWeights
{
    public int enemy { get; set; } = 100;
    public int landmark { get; set; } = 20;
    public int treasure { get; set; } = 20;
    public int _default { get; set; } = 100;
    public int empty { get; set; } = 50;
    public int trap { get; set; } = 20;
    public int props { get; set; } = 20;
    public SpawnWeights() { }
}

public struct MapConfig
{

    public int Length { get; set; } = 1000;
    public int CollisionRadius { get; set; } = 5;
    public int Thickness { get; set; } = 1;
    public int? Seed { get; set; } = null;
    public bool FlagBoss { get; set; } = true;
    public bool FlagQuest { get; set; } = true;
    public MapConfig() { }
}

public class NodeContainerData
{
    public HashSet<Vect2D> NodesFloor { get; set; } = new();
    public HashSet<Vect2D> NodesFloorRaw { get; set; } = new();
    public Dictionary<Vect2D, TileSpawnType> NodesGenerate { get; set; } = new();
    public Dictionary<Vect2D, TileSpawnType> NodesObjects { get; set; } = new();
    public NodeContainerData() { }
}

public readonly struct Vect2D : IEquatable<Vect2D>
{
    public readonly int x;
    public readonly int y;
    public Vect2D(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public bool Equals(Vect2D other) => x == other.x && y == other.y;
    public override bool Equals(object? obj) => obj is Vect2D other && Equals(other);
    public override int GetHashCode() => System.HashCode.Combine(x, y);

    public static bool operator ==(Vect2D left, Vect2D right) => left.Equals(right);
    public static bool operator !=(Vect2D left, Vect2D right) => !left.Equals(right);

    public override string ToString() => $"{x},{y}";
}

public enum TileSpawnType
{
    Empty = 0,
    Default = 1,
    StartGenerator = 2,
    EndGenerator = 3,
    TreasureGenerator = 4,
    EnemyGenerator = 5,
    LandmarkGenerator = 6,
    BossGenerator = 7,
    QuestGenerator = 8,
    DefaultGenerator = 9,
    EmptyGenerator = 10,
    TrapGenerator = 11,
    TrapObject = 40,
    TreasureObject = 50,
    EnemyCollector = 60,
    EnemyObject = 61,
    BossObject = 62,
    MainBossObject = 63,
    LandmarkObject = 70,
    QuestObject = 80,
    PropObject = 90,
    StartObject = 100,
    EndObject = 101,
    WaterTile = 500,
    LavaTile = 501,
}

public struct GenerateObjectWeights
{
    public int EmptyWeight { get; set; }
    public int PropWeight { get; set; }
    public int EnemyWeight { get; set; }
    public int LandmarkWeight { get; set; }
    public int TreasureWeight { get; set; }
    public int TrapWeight { get; set; }
    public int DefaultWeight { get; set; }
    public int TotalWeight => PropWeight + EnemyWeight + LandmarkWeight + TreasureWeight + TrapWeight + EmptyWeight + DefaultWeight;
    public TileSpawnType GetRandomObject(Random rng)
    {
        if (TotalWeight <= 0)
            return TileSpawnType.Empty;
        int roll = rng.Next(TotalWeight);
        if (roll < EmptyWeight) return TileSpawnType.Empty;
        roll -= EmptyWeight;
        if (roll < PropWeight) return TileSpawnType.PropObject;
        roll -= PropWeight;
        if (roll < EnemyWeight) return TileSpawnType.EnemyObject;
        roll -= EnemyWeight;
        if (roll < LandmarkWeight) return TileSpawnType.LandmarkObject;
        roll -= LandmarkWeight;
        if (roll < TreasureWeight) return TileSpawnType.TreasureObject;
        roll -= TreasureWeight;
        return TileSpawnType.TrapObject;
    }
}