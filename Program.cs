namespace MapGeneratorCs
{
    class Program
    {

        static void Main(string[] args)
        {
            var map = new MapConstructor(enableDetailedLogging: false);

            if (args.Length == 0)
                Console.WriteLine("No arguments provided. config files generated for customization\n  type --help for commands.");

            if (args.Length > 0 && args[0] == "--help")
            {
                Console.WriteLine("Available commands:\n" +
                    "  --reset - Initialize configuration files with default values.\n" +
                    "  --json - Generate map and save as JSON file.\n" +
                    "  --image - Generate map and save as image file.\n" +
                    "  --all - Generate map and save as both JSON and image files.\n" +
                    "  --help - Show this help message."
                );
                
                
                return;
            }

            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "--reset":
                        ConfigLoader.InitConfigFiles("config", overwriteExisting: true);
                        break;
                    case "--json":
                        map.GenerateMap();
                        map.SaveMapAsJson();
                        break;
                    case "--image":
                        map.GenerateMap();
                        map.SaveMapAsImage();
                        break;
                    case "--all":
                        map.GenerateMap();
                        map.SaveMapAsJson();
                        map.SaveMapAsImage();
                        break;
                    default:
                        Console.WriteLine($"Unknown command: {args[0]}\nType --help for a list of commands.");
                        break;
                }
            }


        }
    }
}
