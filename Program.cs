// File: Program.cs

namespace MapGeneratorCs
{
    class Program
    {
        static void Main(string[] args)
        {
            var map = new MapConstructor(
                length: 1_000,
                thickness: 1,
                collisionRadius: 3,
                seed: new Random().Next(),
                spawnFactors: (
                    enemyFactor: 2,
                    landmarkFactor: 1,
                    treasureFactor: 1,
                    emptyFactor: 1,
                    defaultFactor: 1,
                    isBoss: true,
                    isQuest: true
                ),
                enableDetailedLogging: false
            );

            map.GenerateMap();
            map.SaveMapAsImage("generated_map.png");
        }
    }
}
