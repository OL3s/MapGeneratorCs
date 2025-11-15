namespace MapGeneratorCs.Types;
public readonly struct Vect2D : IEquatable<Vect2D>
{
    public readonly int x;
    public readonly int y;
    public Vect2D(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public float DistanceTo(Vect2D other)
    {
        return (float)Math.Sqrt((x - other.x) * (x - other.x) + (y - other.y) * (y - other.y));
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