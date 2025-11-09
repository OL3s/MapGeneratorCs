# Map Generation System

Procedural map generation utilities and tests for the MapGeneratorCs project.

## Quick start

Download .NET SDK ver 8.0 or later **[here](https://dotnet.microsoft.com/en-us/download)**

Build the solution:
```sh
dotnet build MapGeneratorCs/
```

Run tests:
```sh
dotnet test MapGeneratorCs.Tests/
```

## Usage (example)
Create and generate a map programmatically:

```csharp
// example usage
var ctor = new MapGeneratorCs.MapConstructor();
ctor.GenerateMap();
ctor.SaveMapAsImage();
ctor.SaveMapAsJson(); // writes export/map_output.json by default
```

Load from JSON into a new instance:

```csharp
var loaded = new MapGeneratorCs.MapConstructor();
loaded.LoadMapFromJson(); // reads export/map_output.json by default
```

For the above methods see:
- [`MapGeneratorCs.MapConstructor.GenerateMap`](MapConstructor/MapConstructor.cs)
- [`MapGeneratorCs.MapConstructor.SaveMapAsJson`](MapConstructor/SubClasses/JsonMapBuilder.cs)
- [`MapGeneratorCs.MapConstructor.LoadMapFromJson`](MapConstructor/MapConstructor.cs)

## Troubleshooting
- If JSON load returns empty data when calling `LoadMapFromJson()` on an instance, ensure the loader copies the deserialized map into `this` (see [`MapGeneratorCs.MapConstructor.LoadMapFromJson`](MapConstructor/MapConstructor.cs) and [`MapGeneratorCs.MapConstructor.JsonMapBuilder.LoadMapFromJson`](MapConstructor/SubClasses/JsonMapBuilder.cs)).  
- Unit test that validates save/load is [MapGeneratorCs.Tests/UnitTest1.cs](MapGeneratorCs.Tests/UnitTest1.cs). If counts differ after load, the instance was likely not updated with the loaded data.

## Notes
- Output files (images / json) are placed under `export/` by default (see implementation in [`MapGeneratorCs.MapConstructor`](MapConstructor/MapConstructor.cs)).  
- Tests expect configuration assets to exist where tests reference them; see [MapGeneratorCs.Tests/UnitTest1.cs](MapGeneratorCs.Tests/UnitTest1.cs).

## License
Hobby project — no license specified.

## More documentation
See module README for the library folder: [MapGeneratorCs/README.md](MapGeneratorCs/README.md)
```