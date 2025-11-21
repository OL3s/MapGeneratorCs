# Overskrift

- Objectbased for each pathfinding type
- Auto-generates landmarks with algorythm
- A* is not always faster, but at most times
- ALT is always faster if you dont include precomputing time
- Dij and ALT always find same path, A* sometimes find a longer path (not by much, few scores) but is better for a dynamic pathfinding tool.
- Diagonal kost for objecter og vegger slik at de ikke går "igjennom" disse diagonalt
- Lenge / Kostnad for bevegelse: 1 / sqrt(2) + objekt-kost

Kjøretid for algoritman
