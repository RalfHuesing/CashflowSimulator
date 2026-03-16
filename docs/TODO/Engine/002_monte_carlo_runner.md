# Monte-Carlo Runner & Cholesky

## Beschreibung
Umsetzung der Simulations-Infrastruktur für korrelierte Zufallspfade.

## Details
- **Korrelationsmatrix:** Aufbau aus `CorrelationEntryDto`.
- **Cholesky-Zerlegung:** Berechnung der unteren Dreiecksmatrix zur Korrelation der Zufallsinnovationen.
- **Seed-Replay-Pattern:** Implementierung des 3-Stufen-Modells (Lightweight → Median-Suche → Heavyweight-Replay) für Speicher-Effizienz.
- **Reproduzierbarkeit:** Sicherstellen, dass Pfade bei gleichem Master-Seed identisch bleiben.

## Status
- [ ] Offen
- [ ] Cholesky-Zerlegung (Math-Lib oder eigene Implementierung)
- [ ] Seed-Management (Sub-Seeds pro Iteration)
- [ ] Replay-Logik im Runner
