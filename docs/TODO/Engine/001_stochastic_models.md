# Stochastische Marktmodelle (Engine)

## Beschreibung
Implementierung der mathematischen Modelle für Marktfaktoren (GBM und OU) in der Engine.

## Details
- **Geometric Brownian Motion (GBM):** Für Aktien/Indizes (Drift + Volatilität).
- **Ornstein-Uhlenbeck (OU):** Für Zinsen/Inflation (Mean-Reversion).
- **Diskrete Approximation:** Monatliche Update-Formeln implementieren.
- **Validierung:** Sicherstellen, dass Parameter (Vola, Drift) in sinnvollen Bereichen liegen.

## Status
- [ ] Offen
- [ ] GBM Modell Kernlogik
- [ ] OU Modell Kernlogik
- [ ] Integration in `SimulationRunner` (Marktdaten-Pfad Erzeugung)
