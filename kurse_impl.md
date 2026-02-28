# Plan für die Implementierung des Features zur Kurs-Abfrage (Stock Price Query)

## Big Picture

Das Feature zur Kurs-Abfrage soll es ermöglichen, den aktuellen Kurs von Finanzinstrumenten basierend auf deren Symbolen abzurufen und zu aktualisieren. **Für die erste Phase wird nur ein Dummy-Provider implementiert**, der zufällige Kurswerte zurückgibt. Dies ermöglicht die Integration und das Testen der UI-Logik, ohne von externen APIs abhängig zu sein.

Der Plan wurde aktualisiert, um den bereits existierenden Stand widerzuspiegeln.

## Aktueller Stand (bereits implementiert)

1. **Schnittstelle `IStockPriceService`** – existiert in `CashflowSimulator.Contracts/Interfaces/IStockPriceService.cs`.
2. **Dummy-Provider `DummyStockPriceProvider`** – existiert in `CashflowSimulator.Infrastructure/Services/DummyStockPriceProvider.cs`; gibt zufällige Kurse zwischen 50 und 200 zurück.
3. **DTO `StockPriceResultDto`** – existiert in `CashflowSimulator.Contracts/Dtos/StockPriceResultDto.cs`; enthält Symbol, Preis, Zeitstempel und Erfolgsstatus.
4. **Dependency Injection** – in `CashflowSimulator.Desktop/Program.cs` ist der `DummyStockPriceProvider` als `IStockPriceService` registriert (Zeile: `services.AddSingleton<IStockPriceService, DummyStockPriceProvider>()`).
5. **Unit-Tests** – `DummyStockPriceProviderTests` existieren in `CashflowSimulator.Infrastructure.Tests/DummyStockPriceProviderTests.cs` und testen alle wichtigen Szenarien.

## 1. Definieren einer Schnittstelle für den Stock Price Service
**Status:** ✅ **Erledigt**

- [x] Erstellen der Datei `IStockPriceService.cs` im Projekt `CashflowSimulator.Contracts/Interfaces`.
- [x] Definition der Methode `GetStockPriceAsync(string symbol)` in der Schnittstelle.

## 2. Implementierung des Dummy-Providers (Default für Phase 1)
**Status:** ✅ **Erledigt**

- [x] Erstellen der Datei `DummyStockPriceProvider.cs` im Projekt `CashflowSimulator.Infrastructure/Services`.
- [x] Implementierung der Klasse `DummyStockPriceProvider`, die die Schnittstelle `IStockPriceService` implementiert.
- [x] Zufällige Kursgenerierung (50–200) für beliebige Symbole; Fehlerbehandlung für leere Symbole.
- [x] Registrierung in der DI-Container (Program.cs) als Standard-Provider.

## 3. Anpassung der DTOs zur Inklusion von Symbolen
**Status:** 🔄 **Teilweise erledigt**

- [x] `StockPriceResultDto` enthält Symbol-Feld.
- [ ] **AssetDto** enthält derzeit nur ISIN (`Isin`). Für eine echte Kursabfrage könnte ein separates `Symbol`-Feld hinzugefügt werden (z. B. für Börsenticker wie "AAPL"). Dies ist für den Dummy-Provider nicht erforderlich, da der Provider mit jedem beliebigen String arbeitet.
- [ ] Optional: Ergänzung der `AssetDto` um `Symbol` (oder Nutzung der ISIN als Symbol). Entscheidung später.

## 4. Erstellen eines Buttons zur Aktualisierung von Kursen
**Status:** ✅ **Erledigt**

- [x] Identifizieren der Feature-View (Portfolio-View).
- [x] Button "Kurse aktualisieren" in der Portfolio-View mit Command-Bindung.
- [x] Bindung an `UpdatePricesCommand`; Button wird während der Abfrage deaktiviert (`!IsUpdatingPrices`).
- [x] Lade-Indikator (ProgressBar) und Status-Text (`UpdateStatus`).

## 5. Implementierung der Logik zur Abfrage von Symbolen und Aktualisierung der Kurse
**Status:** ✅ **Erledigt**

- [x] Logik im PortfolioViewModel: alle Assets aus dem Projekt, Symbol = ISIN (Fallback: Name).
- [x] Aufruf von `IStockPriceService.GetStockPriceAsync(symbol)` (Dummy-Provider) pro Asset.
- [x] Aktualisierung von `CurrentPrice` und `CurrentValue` (Stückzahl × Kurs) im Projekt via `UpdatePortfolio`.
- [x] Fehlerbehandlung und Benutzerfeedback über `UpdateStatus`; `NotifyCanExecuteChangedFor(UpdatePricesCommand)` damit der Button nach Abschluss wieder aktiv wird.

## 6. Handhabung von Provider-Agnostizität
**Status:** ✅ **Grundlage gelegt**

- [x] Schnittstelle `IStockPriceService` definiert.
- [x] Dependency Injection ermöglicht Austausch des Providers zur Laufzeit.

## 7. Testing
**Status:** ✅ **Erledigt**

- [x] Unit-Tests für den Dummy-Provider vorhanden (`DummyStockPriceProviderTests`).
- [x] Integrationstests für die Kursaktualisierungs-Logik (ViewModel) in `PortfolioViewModelPriceUpdateTests`: erfolgreiche Aktualisierung (CurrentPrice/CurrentValue), keine Assets, Fallback ISIN→Name, Fehlerfall, CanExecute.
- [ ] Optional: UI-Tests für den Aktualisierungs-Button.

## Nächste Schritte (Priorisierung)

1. ~~**Button und Logik für Kursaktualisierung** (Punkte 4 und 5)~~ – erledigt.
2. **Eventuell Symbol-Feld in AssetDto ergänzen** – falls ISIN für echte APIs nicht ausreicht.
3. ~~**Weitere Tests** (ViewModel-Logik)~~ – erledigt (`PortfolioViewModelPriceUpdateTests`).
4. Optional: UI-Tests für den Aktualisierungs-Button.

## Hinweise

- Der Dummy-Provider ist voll funktionsfähig und kann sofort verwendet werden.
- Die Architektur erlaubt den späteren Austausch des Providers ohne Änderungen an der UI-Logik.
- Alle fachlichen Regeln (Validierung) bleiben im Validation-Projekt; die Kursabfrage ist ein reiner Datenbeschaffungsdienst.
