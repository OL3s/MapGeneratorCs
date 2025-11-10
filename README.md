# Map Generation System

A C# library for generating, exporting and loading procedural maps.
Note: export/ and generated images are in .gitignore by default.

## Quick start
1. Install .NET 8 SDK.
2. Build:
```sh
dotnet build MapGenerationCs.sln
```
3. Run tests:
```sh
dotnet test tests/MapGeneratorCs.Tests/MapGeneratorCs.Tests.csproj
```

## Usage (example)
Create, generate and export:
```csharp
var ctor = new MapGeneratorCs.MapConstructor();
ctor.GenerateMap();                                   // generates the map
ctor.SaveMapAsImage();                                // creates export/map_output.png
ctor.SaveMapAsJson();                                 // creates export/map_output.json
```

Load from JSON into a new instance:
```csharp
var loaded = new MapGeneratorCs.MapConstructor().LoadMapFromJson();
```

## License
Hobby project — no license specified.