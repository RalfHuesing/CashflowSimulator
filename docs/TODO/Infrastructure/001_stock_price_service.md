# Kurs-Abfrage Erweiterung

## Beschreibung
Erweiterung des Stock Price Service für bessere Flexibilität und Integration.

## Details
- **Symbol-Suche:** Ermittlung von Symbolen, wenn diese nicht vorhanden sind.
- **Provider-agnostisch:** Unterstützung für verschiedene Anbieter (austauschbare Provider).
- **Engine-Integration:** Anbindung in die Simulations-Engine.
- **Datenmodell:** DTOs ggf. um Symbole erweitern (Kurse/ISIN vs. Symbole).

## Status
- [x] Phase 1 umgesetzt (`IStockPriceService`, `DummyStockPriceProvider`, Button im Portfolio)
- [ ] Symbol-Suche implementieren
- [ ] Provider-Infrastruktur flexibilisieren
- [ ] Simulations-Engine-Anbindung
