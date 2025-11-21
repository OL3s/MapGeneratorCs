# Innlevering Øving 7 – IDATT2101

**Medlemmer:** `Ole-Kristian Wigum`, `Casper Kolsrud`

# Innledning
Vi fikk godkjenning fra foreleser til å integrere denne oppgaven i vårt eksisterende videospillprosjekt. Dette inkluderer både pathfinding og kartgenerering. Rapporten går kort gjennom kartgenerering, før vi forklarer hvordan pathfinding implementeres og brukes. Koden er bygget på objektorientert struktur.

# Kartgenerering
Kartet genereres basert på antall noder. En pipeline lager spilleområde, objekter og start/slutt-posisjoner. I pathfinding er en node uten objekt billigere å traversere enn en node med objekt (start og slutt koster ingenting).

Her er kostnadsøkningen for objekter:

``` cs
public static float GetNodeMovementPenalty(TileSpawnType type) =>
    type switch
    {
        TileSpawnType.Default => 0f,
        TileSpawnType.TrapObject => 3f,
        TileSpawnType.TreasureObject => 100f,
        TileSpawnType.LandmarkObject => 100f,
        TileSpawnType.PropObject => 100f,
        _ => 10f
    };
```

Kostnaden for et steg = basekost (1 eller √2 ved diagonal) + movement penalty.

# Generering av pathfinding
Vi har en objektorientert løsning der hver algoritme er et eget objekt. Disse bruker `PathNodes`, generert av `MapConstructor`.

``` cs
public class PathNodes : Dictionary<Vect2D, PathNode>
{
    public PathNodes() : base() { }
    public PathNodes(IDictionary<Vect2D, PathNode> pathNodes) : base(pathNodes) { }

    public Vect2D Dimentions {
        get {
            var maxX = this.Keys.Max(p => p.x);
            var maxY = this.Keys.Max(p => p.y);
            return new Vect2D(maxX + 1, maxY + 1);
        }
    }
}
```

`PathNode`:

``` cs
public class PathNode
{
    public Vect2D Position;
    public TileSpawnType NodeType = TileSpawnType.Default;
    public float MovementPenalty = 0;
    public HashSet<PathNode> Neighbours = new();
    public float TotalCost => CostFromStart + HeuristicCost;

    public float CostFromStart = float.MaxValue;  
    public float HeuristicCost = float.MaxValue;  
    public PathNode? ParentNode = null;
}
```

Algoritmene `ALTGenerator`, `AStarGenerator` og `DijGenerator` bruker disse nodene.

# Algoritmene

## DijGenerator
Bruker Dijkstra-algoritmen. I prosjektet brukes den primært til å generere precomputations for ALT.  
Når man trenger en ren dijkstra-sti i oppgaven, brukes `DijUtils.CreateDijPathFromPathNodes()`.

## A*
I skoleoppgaven skal ALT og Dijkstra sammenlignes, men i spillet trenger vi dynamisk pathfinding.  
ALT er statisk og krever prekompilering.

**Derfor:**
- A* brukes for dynamiske kall (kart endrer seg).
- ALT brukes for store beregninger ved oppstart.

A* er retningsbasert og raskere enn Dijkstra, med mindre korteste vei faktisk er en omvei.

## ALT
ALT bruker Dijkstra-prekompilering og genererte landemerker (optimalisert spredning).  
Heuristikken er basert på reelle kostnader, ikke luftlinje, og gir bedre estimater.

# Resultater og sammenligning

Test med **8 millioner noder**:

``` powershell
dotnet run 8000000
```

Resultat:

``` powershell
School Assignment Important Info:
  - A*: 12157 ms, length 3112,6934
  - Dijkstra Raw: 23158 ms, length 3112,6934
  - ALT: 2507 ms, length 3112,6934
  - Closest Objects: 2543,1962, 2553,1987, 2554,1987, 2552,1967, 2538,1959
```

Alle finner samme optimale sti.  
Dijkstra utforsker mest → tregest.  
A* bruker retning → raskere.  
ALT bruker precompiled distanser → raskest.

### Visualiseringer
![alt text](image.png)

*Figur 1: Korteste vei funnet av AStar. dijk og ALT finner samme vei så vi trenger kun 1 bilde*

![alt text](image-1.png)

*Figur 2: Lilla ruter viser de nærmeste interessepunktene. Blåst opp for simpelhet*

![alt text](image-2.png)

*Figur 3: Grønn node representerer startsted. Gul noder representerer interessepunkter*

Viktig! Blå/mørkeblå noder er hindringer, og har da en høyere cost/avstand fra startstedet.

### Komplikasjoner – diagonale hindringer
Viktig utfordring: unngå "klipping" gjennom hjørner på objekter/vegger.

Løsning (utdrag):

``` cs
public static float CalculateCornerPenalty(PathNodes nodes, Vect2D from, Vect2D to)
{
    if (from.x == to.x || from.y == to.y)
        return 0f;

    var a = new Vect2D(to.x, from.y);
    var b = new Vect2D(from.x, to.y);

    if (```nodes.TryGetValue(a, out var aNode) || ```nodes.TryGetValue(b, out var bNode))
        return 0f;

    return aNode.MovementPenalty + bNode.MovementPenalty;
}
```
## Queue priority count
Vi har oversett at oppgaven har i krav om å ha med en teller på hvor mange noder den faktisk går igjennom når den gjennomfører QUERIES. her er et resultat på et nytt kart, bare at den også genererer tallene for NODER BESØKT

# Sammendrag
Prosjektet genererer et prosedyrekart og sammenligner Dijkstra, ALT og A*. Fokus er på algoritmenes arbeidsmetode, tidsbruk og nodeoppførsel. Vi viser også fem interessepunkter som krav.

I tillegg har vi inkludert en teller som viser hvor mange noder algoritmene besøker under queries. Dette gir en bedre forståelse av effektiviteten til hver algoritme i praksis.

*Merk:* cost/length = tid (1 cost = 1 sekund).
