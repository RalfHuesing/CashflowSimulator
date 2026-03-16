# Steuer-Engine (Deutschland)

## Beschreibung
Umsetzung der steuerlichen Berechnungslogik gemäß deutschem Recht.

## Details
- **FIFO-Verbrauch:** Implementierung der Logik zum Verbrauch von `AssetTranches` bei Verkauf.
- **Kapitalertragsteuer (KapESt):** Berechnung der Steuer auf realisierte Gewinne (inkl. Soli + ggf. KiSt).
- **Teilfreistellung:** Berücksichtigung des `TaxType` (z.B. 30 % bei Aktienfonds).
- **Vorabpauschale:** Jährliche Berechnung der Vorabpauschale auf thesaurierende Fonds.
- **Verlustverrechnungstopf:** (Später) Tracking von Verlusten zur Verrechnung mit Gewinnen.

## Status
- [ ] Offen
- [ ] FIFO Logic (`Engine.Services.Tax`)
- [ ] KapESt Berechnung
- [ ] Vorabpauschale Logik
