namespace MapGeneratorCs
{
    class Program
    {
        private static readonly int _length = 1000;
        private static readonly int _thickness = 1;
        private static readonly int _collisionRadius = 5;

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
                length: args.Length > 0 ? int.Parse(args[0]) : _length,
                thickness: args.Length > 1 ? int.Parse(args[1]) : _thickness,
                collisionRadius: args.Length > 2 ? int.Parse(args[2]) : _collisionRadius,
                seed: new Random().Next(),
                flags: (isBoss: true, isQuest: true),
                enableDetailedLogging: false
            );

            map.GenerateMap();
            map.SaveMapAsImage("export/map_output.png");
        }
    }
}
