# Innvelering oving 7 - IDATT2101

**Medlemmer:** `Ole-Kristian Wigum`, `Casper Kolsrud`

- Objectbased for each pathfinding type
- Auto-generates landmarks with algorythm
- A* is not always faster than dij, but almost every time. Directionized
- ALT is always faster if you dont include precomputing time
- Diagonal kost for objecter og vegger slik at de ikke går "igjennom" disse diagonalt
- Lenge / Kostnad for bevegelse: 1 / sqrt(2) + objekt-kost

Kjøretid for algoritman


# Innledning

Vi fikk godkjenning av foreleser å integrere denne oppgaven i mitt nåværende videospill. Dette inkluderer både pathfinding, men også generering av kart. Rapporten går fort igjennom generering av kartet, for å så forklare alle de ulike måtene pathfinding blir brukt som løsning. Vi blir å forklare kartgenerering i hensyn til pathfinding.
Koden er sterkt tilknyttet objektorientert kodestruktur

## Kart Generering
Koden genererer et kart basert på antall noder. En pipeline prosess fullfører alt som trengs til kartet, alt fra spilleområde, til objekter, og start/slutt posisjon på kartet. I pathfinding blir en plassering som ikke er et objekt "billigere" å traversere igjennom enn en plassering som som har objekt på seg (start og slutt objektet koser ingenting).

Her har vi kostnadsøkningen av objekter i koden
``` cs
public static float GetNodeMovementPenalty(TileSpawnType type) =>
    type switch
    {
        // Movementcost = (1, sqrt(2) for diagonal) + GetNodeMovementPenalty()
        TileSpawnType.Default => 0f,
        TileSpawnType.TrapObject => 3f,
        TileSpawnType.TreasureObject => 100f,
        TileSpawnType.LandmarkObject => 100f,
        TileSpawnType.PropObject => 100f,
        _ => 10f
    };
```
Objektet har funksjoner i seg for cloning, generering osv. Kostnaden fra en node til en annen node er da basekost (som er 1, eller sqrt(2) om den er diagonal) + denne MOVEMENT PENALTY

## Generering av pathfinding
Ved at vi har fokusert på en objektorientert kodeløsning, så har vi separert hver algorytme som et objekt vær. Disse objektene er basert på objektet PathNodes som er generert i MapConstruktøren. Her er objektet:
``` cs
public class PathNodes : Dictionary<Vect2D, PathNode>
{
    public PathNodes() : base( ) { }
    public PathNodes(IDictionary<Vect2D, PathNode> pathNodes) : base(pathNodes) { }
    public Vect2D Dimentions
    {
        get
        {
            var maxX = this.Keys.Max(position => position.x);
            var maxY = this.Keys.Max(position => position.y);
            return new Vect2D(maxX + 1, maxY + 1);
        }
    }
}
```

Pathnodes er basert på en Dictionary av PathNode der lokalisjonen er nøkkelen.
``` cs
public class PathNode
{
    // == PRECOMPILED == see CreatePathNodesFromMap();
    public Vect2D Position;
    public TileSpawnType NodeType = TileSpawnType.Default;
    public float MovementPenalty = 0;
    public HashSet<PathNode> Neighbours = new();
    public float TotalCost => CostFromStart + HeuristicCost;

    // == RUNTIME == see AStar and Dij methods
    public float CostFromStart = float.MaxValue;  
    public float HeuristicCost = float.MaxValue;  
    public PathNode? ParentNode = null;
}
```

Videre bruker da klassene, `ALTGenerator`, `AStarGenerator` og `DijGenerator` denne PathNodes klassen for å finne kart, vi fant ut at dette ver en optimal løsning.

Vi kan nå gå igjennom de diverse objektene for pathfinding:

### DijGenerator

DijGenerator er basert på `dijkstra` algorytmen, I denne sammenhengen blir dette bare brukt for å generere fulle dijkstra kaluleringer for ALT algorytmen.

I oppgaven er det vedt om å finne direkte tid når man bruker en dijksta, da bruker ikke denne generatoren, men istedenfor `DijUtils.CreateDijPathFromPathNodes()` istedenfor da den i hovedsakk ikke skal viderebrukes i spill prosjektet.

### AStar

I skoleoppgaven skal vi i hovedsak sammenligne ALT og Dijkstra, men i prosjektet så er det essensielt at vi har en pathfinding algorytme som også kan fungere dynamisk, da ALT er en statisk løsning som trenger prekompilering. MED ANDRE ORD:
- AStar for dynamiske kall (om objekter ødelegges og kart forandres osv i spilletid)
- ALT brukes for store kalkuleringeringer når spillet starter.

Derfor har vi bestemt oss for å ta med en ren dynamisk AStar i oppgaven, og tar også med TIDSKJØRINGER med denne teknikken.

**Hvordan AStar fungerer**

AStar KONTRA bruk av en rå dijkstra, er at den er retningsbasert, der dijkstra er spredningsbasert fra startsposisjon. Dette vil (nesten) altid gi bedre KALKULERINGSTID, memindre den raskeste vei faktisk er en omvei.

### ALT

ALT objektet bruker da DijGenerator for å generere sine landemerker, merk her at det også er laget en algorytme til å finne opptimale landemerker, slik at de blir godt spredt ut over kartet.

**Hvordan ALT fungerer**
ALT er en utbygging av AStar algorytmen, men istedenfor å bruke luftlinje for avstandskalkulering, bruker den da den lagrede PathNodes i DijstrakConstruktøren sine precompilerte lengdeverdier. Dette gir en mer realistisk lengdeastimat i kalkulasjonen.

## Resultater og sammenligning

Nå som vi har beskrevet de forskjellige algorytmene, kan vi se på de faktiske resultatene, om de holder til som planlagt. I vårt test eksperiment, genererer vi 8 millioner noder, og får noen resulater basert på dette.
``` cs
dotnet run 8000000
```
Resultatet på denne er:
``` powershell
School Assignment Important Info:
  - A*: 12157 ms, length 3112,6934
  - Dijkstra Raw: 23158 ms, length 3112,6934
  - ALT: 2507 ms, length 3112,6934
  - Closest Objects: 2543,1962, 2553,1987, 2554,1987, 2552,1967, 2538,1959
```
Som vi ser, så har vi fått de resultatene som er forventet. Heuristikken i A*, ALT og Dijkstra er korrekt, fordi de finner den samme, optimale stien (Length 1335,1643). Dijkstra utforsker mest av kartet og bruker derfor mest tid, A* er retningsbasert, og slipper å lete gjennom hele kartet som gjør denne algoritmen litt raskere enn dijkstra. ALT bruker faktiske lengder basert på de genererte landemerkene og finner den samme optimale stien raskest. "Closest objects" er koordinatene for fem interessepunkter som ALT bruker i evalueringen.

Vi bestemte oss for at den letteste løsningen visuelt, er å generere bilder basert på objektene, med forskjellige metoder for å generere bilder, disse finner du i util klassen `Imagify`.

### Skjermbilder med forklaring
![alt text](image.png)

*Figur 1: Korteste vei funnet av AStar. dijk og ALT finner samme vei så vi trenger kun 1 bilde*

![alt text](image-1.png)

*Figur 2: Lilla ruter viser de nærmeste interessepunktene. Blåst opp for simpelhet*

![alt text](image-2.png)

*Figur 3: Grønn node representerer startsted. Gul noder representerer interessepunkter*

Viktig! Blå/mørkeblå noder er hindringer, og har da en høyere cost/avstand fra startstedet.

### Komplikasjoner
Noe som ble en ekstra utfordring i denne oppgaven var å få en realistisk vei til DET MÅLET MAN SØKTE ETTER når det ankommer diagonale noder. grunnen til dette er at veien ikke skal gå diagonalt "igjennom" objekter eller vegger". Om dette er RESULTATET, så må veien KJØRE rundt EVENTUELT VEGG ELLER OBJEKT istedenfor. I tilleg så må dette reflekteres i kostnadden, om det er vegg over en diagonal DIRECTION, så fjernes nå denne noden fra nabonodene, men om det er et objekt i veien diagonals, så legges dette til SCOREN, slik at det blir samme kostnad som om man skulle gå på objektet. Det tok tid for å finne en god løsning på denne problemstillingen, men kom fram til en fungerende løsning for begge, her er et kort ustnitt av hvordan det ble gjort på objekter som har diagonal tilnærmining.
``` cs
    public static float CalculateCornerPenalty(PathNodes nodes, Vect2D from, Vect2D to)
    {
        // Only applies to diagonal moves
        if (from.x == to.x || from.y == to.y)
            return 0f;

        var a = new Vect2D(to.x, from.y); // horizontal corner
        var b = new Vect2D(from.x, to.y); // vertical corner

        // Neighbours are created only if both exist, but be safe
        if (!nodes.TryGetValue(a, out var aNode) || !nodes.TryGetValue(b, out var bNode))
            return 0f;

        return aNode.MovementPenalty + bNode.MovementPenalty;
    }
```

## Sammendrag (1‑2 setninger)

Prosjektet er et prosedyregenerert kart og sammenligner dijkstra, ALT og A*. Prosjektet er i hovedsak et hobbyprosjekt, men hvordan dette prosjektet besvarer oppgaven blir forklart her. Fokuset i denne rapporten er arbeidsmetoden til hver algoritme, tidsbruk og hvordan nodene oppfører seg. I tillegg skal vi vise 5 interessepunkt av et passende sted (oppgavekriterie).


Viktig! Tid/minutt/sekund er forklart med cost/length - 1 "cost" = 1 sekund. Hver node har en cost-verdi, og den viser hvor lang tid det tar å kjøre "gjennom" noden.
Eksempel: bilen kjører gjennom en node med cost-verdi "1", det tar 1 sekund å kjøre "gjennom" noden. Neste node har en cost-verdi på "30", det tar derfor 30 sekunder å kjøre gjennom. Printen har den totale cost-verdien av veien bilen har kjørt. I printen er cost = length.



## Prosjektoppsett
- Kartet er basert på objektet MapConstructor: (link)
- Kartet genereres ved funcksjonen .GenerateMap(), eller kan lastes inn med .LoadMapFromJson()
- Pathnodes er bygd med funksjonen .GeneratePathNodes() disse er brukt i pathfinding.
- Tilgjengelige pathfinding algoritmer:
  - A*  
  - Dijkstra (raw)
  - Dijkstra (precompute)
  - ALT

## Mål / målinger
- Tid brukt (ms) for å finne sti og lengde på stien
- Path‑kost/length (som rapportert av PathResult).
- Antall noder trukket fra prioritetskø (lages etterpå).
- Visualiseringer lagres som kart i ConsoleClientApp.

## Resultat (8 millioner nodes)
``` powershell
School Assignment Important Info:
  - A*: 12157 ms, length 3112,6934
  - Dijkstra Raw: 23158 ms, length 3112,6934
  - ALT: 2507 ms, length 3112,6934
  - Closest Objects: 2543,1962, 2553,1987, 2554,1987, 2552,1967, 2538,1959
```
## Kort forklaring av pathfinding (1 avsnitt)
Heuristikken i A*, ALT og Dijkstra er korrekt, fordi de finner den samme, optimale stien (Length 1335,1643). Dijkstra utforsker mest av kartet og bruker derfor mest tid, A* er retningsbasert, og slipper å lete gjennom hele kartet som gjør denne algoritmen litt raskere enn dijkstra. ALT bruker faktiske lengder basert på de genererte landemerkene og finner den samme optimale stien raskest. "Closest objects" er koordinatene for fem interessepunkter som ALT bruker i evalueringen.


## Referanser (filer/symboler)
Se overnevnte symboler og:
- Kjør via:
