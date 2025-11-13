# Map Generation System

A small C# library and CLI for generating, exporting and loading procedural maps.

## Quick start

1. Install .NET 8 SDK.
2. Build the solution:
   ```sh
   dotnet build MapGenerationCs.sln
   ```
3. Run unit tests:
   ```sh
   dotnet test tests/MapGeneratorCs.Tests/MapGeneratorCs.Tests.csproj
   ```
   - The large/integration test is gated behind the environment variable `RUN_HUGE_TESTS=1`.

## Run the console example

The console app demonstrates map generation and pathfinding:

- App entry: [src/ConsoleClientApp/Program.cs](src/ConsoleClientApp/Program.cs)

Run:
```sh
dotnet run --project src/ConsoleClientApp/ConsoleClientApp.csproj
```

## Core API (examples)

- Create, generate and export a map:
```csharp
var ctor = new MapGeneratorCs.MapConstructor();
ctor.GenerateMap();               // generate
ctor.SaveMapAsImage();            // export/map_output.png
ctor.SaveMapAsJson();             // export/map_output.json
```
- Load a saved map:
```csharp
var loaded = new MapGeneratorCs.MapConstructor();
loaded.LoadMapFromJson(); // loads from export/map_output.json by default
```

Useful types:
- [`MapGeneratorCs.MapConstructor`](src/MapGeneratorCs/MapConstructor/MapConstructor.cs) — main generator and helpers.
- [`MapGeneratorCs.PathFinding.AStar.AStarGenerator`](src/MapGeneratorCs/PathFinding/AStart/AStarGenerator.cs) — run pathfinding on generated maps.
- [`MapGeneratorCs.Generator.Utils.ConfigLoader`](src/MapGeneratorCs/MapConstructor/Components/ConfigLoader.cs) — load/create default config files.

## Config & output

- Default config files are created under `config/` (if missing) by the library. Example config: `configMapConfig.json`
- Output files are written to `export/` (images and JSON). Note: `export/` and generated images are ignored by git by default.

## Tests

- Unit tests: [tests/MapGeneratorCs.Tests/MapGeneratorCs.Tests.csproj](tests/MapGeneratorCs.Tests/MapGeneratorCs.Tests.csproj)
- Integration: [tests/MapGeneratorCs.Tests/PathFindingOnMapTest.cs](tests/MapGeneratorCs.Tests/PathFindingOnMapTest.cs) generates a full map and runs pathfinding; enable heavy tests with `RUN_HUGE_TESTS=1`.

## Notes

- This is a hobby project — no license specified. This means "all rights reserved" by default.
- The repository .gitignore already excludes `export/`, `config/` and generated images.