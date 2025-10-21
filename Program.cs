// File: Program.cs
using System;

namespace MapGeneratorCs
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Map Generator Started");
            new MapConstructor(200, 1, 3, 1, 2, true, true);
        }
    }
}
