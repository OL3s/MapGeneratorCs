namespace MapGeneratorCs
{
    class Program
    {
        static void Main(string[] args)
        {
            string argFirst = args.Length > 0 ? args[0].ToLower() : "";
            if (argFirst == "-h" || argFirst == "help" || argFirst == "--help")
            {
                Console.WriteLine("Usage: MapGeneratorCs <length> <thickness> <collisionRadius>");
                Console.WriteLine("  <length>            : Length of the map (default: 1000)");
                Console.WriteLine("  <thickness>         : Thickness of the paths (default: 5)");
                Console.WriteLine("  <collisionRadius>   : Collision radius (default: 3)");
                Environment.Exit(0);
            }

            var map = new MapConstructor(
                length: args.Length > 0 ? int.Parse(args[0]) : 1_000,
                thickness: args.Length > 1 ? int.Parse(args[1]) : 1,
                collisionRadius: args.Length > 2 ? int.Parse(args[2]) : 4,
                seed: new Random().Next(),
                spawnFactors: (
                    enemyFactor: 1,
                    landmarkFactor: 1,
                    treasureFactor: 1,
                    emptyFactor: 1,
                    defaultFactor: 1,
                    trapFactor: 1,
                    isBoss: true,
                    isQuest: true
                ),
                enableDetailedLogging: false
            );

            map.GenerateMap();
            map.SaveMapAsImage("export/map_output.png");
        }
    }
}
