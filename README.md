# Map Generation System

Procedural map generation + multiple pathfinding strategies (A*, Dijkstra single-target, Dijkstra full precompute, ALT landmarks).

## Quick start

```sh
dotnet build   [src/MapGeneratorCs/MapGeneratorCs.csproj](optional)
dotnet test    [tests/MapGeneratorCs.Tests/MapGeneratorCs.Tests.csproj](optional)
dotnet run --project src/ConsoleClientApp/ConsoleClientApp.csproj
```

## Core usage

Generate & export:
```csharp
var ctor = new MapGeneratorCs.MapConstructor();
ctor.GenerateMap();
ctor.GeneratePathNodes();                    // builds graph
ctor.SaveMapAsImage();
ctor.SaveMapAsJson();
```

Load existing:
```csharp
var loaded = new MapGeneratorCs.MapConstructor();
loaded.LoadMapFromJson(); // default export/map_output.json
loaded.GeneratePathNodes();
```

Pathfinding:
```csharp
var aStar = new MapGeneratorCs.PathFinding.AStar.AStarGenerator(loaded.PathNodes);
var pathA = aStar.FindPath(loaded.StartPosition, loaded.EndPosition);

var dijPre = new MapGeneratorCs.PathFinding.Dijkstra.DijGenerator(loaded.PathNodes, loaded.StartPosition);
var pathDijPre = dijPre.FindPath(loaded.EndPosition);

var pathDijRaw = MapGeneratorCs.PathFinding.Dijkstra.Utils.DijUtils.CreateDijPathFromPathNodes(
    loaded.PathNodes, loaded.StartPosition, loaded.EndPosition);

var alt = new MapGeneratorCs.PathFinding.ALT.ALTGenerator(loaded.StartPosition, landmarkCount: 5, loaded.PathNodes);
var pathAlt = alt.FindPath(loaded.StartPosition, loaded.EndPosition);
```

Image export (cost maps & landmarks):
```csharp
MapGeneratorCs.PathFinding.Image.PathImagify.SavePathToImage(loaded.PathNodes, pathA, "a_star.png");
MapGeneratorCs.PathFinding.Image.PathImagify.SavePathValuesToImage(loaded.PathNodes, dijPre.Dist, "dijkstra_cost.png");
MapGeneratorCs.PathFinding.Image.PathImagify.SavePointOfInterestToImage(
    loaded.PathNodes, alt.Landmarks.GetPositions(), "alt_landmarks.png");
```

## Algorithms

- A* (non-mutating per-run dictionaries): [`AStarUtils.FindPath`](src/MapGeneratorCs/PathFinding/AStart/Components/AStartUtils.cs)
- Dijkstra single-target: [`DijUtils.CreateDijPathFromPathNodes`](src/MapGeneratorCs/PathFinding/Dijkstra/Components/DijUtils.cs)
- Dijkstra full precompute (stores `Dist` + `Parent` only): [`DijGenerator`](src/MapGeneratorCs/PathFinding/Dijkstra/DijGenerator.cs)
- ALT (landmark-based heuristic): [`ALTGenerator`](src/MapGeneratorCs/PathFinding/ALT/ALTGenerator.cs)

All reuse immutable neighbor sets created in: [`PathFindingUtils.GetNeighbours`](src/MapGeneratorCs/PathFinding/Tools/PathFindingUtils.cs)

## Performance notes

- Graph cloning removed (except implicit distance captures inside landmarks).
- Precompute Dijkstra once if many queries from same start.
- ALT improves long-path A* by tighter heuristic; landmark count 4–8 is usually enough.
- Image saving guards huge sizes (see [`PathImagify.SavePathValuesToImage`](src/MapGeneratorCs/PathFinding/Tools/PathImagify.cs)).

## Key types

- Map + generation: [`MapConstructor`](src/MapGeneratorCs/MapConstructor/MapConstructor.cs)
- Graph nodes: [`PathNodes`](src/MapGeneratorCs/PathFinding/Tools/PathFindingTypes.cs)
- Utilities: [`PathFindingUtils`](src/MapGeneratorCs/PathFinding/Tools/PathFindingUtils.cs)
- Config loader: [`ConfigLoader`](src/MapGeneratorCs/MapConstructor/Components/ConfigLoader.cs)

## Tests

- Unit: [`MapGeneratorCs.Tests`](tests/MapGeneratorCs.Tests/MapGeneratorCs.Tests.csproj)
- Pathfinding integration: [`PathFindingOnMapTest.cs`](tests/MapGeneratorCs.Tests/PathFindingOnMapTest.cs)

## Notes

Outputs in `export/` and configs in `config/` are git-ignored. No license specified (all rights reserved). Keep landmark count modest for very large maps.
