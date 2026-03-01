# CashflowSimulator

![License](https://img.shields.io/github/license/RalfHuesing/CashflowSimulator)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey)
![Status](https://img.shields.io/badge/status-pre--alpha-red)

> **⚠️ WICHTIGER HINWEIS: PROJEKT IM AUFBAU**
>
> Dieses Projekt befindet sich aktuell in einer frühen Entwicklungsphase (**Pre-Alpha**).
> Die Software ist **noch nicht funktionsfähig** und dient derzeit vor allem der Entwicklung und Architektur-Validierung. Es gibt noch keine ausführbaren Releases.
>
> *Schau gerne später wieder vorbei oder folge dem Projekt, um Updates zu erhalten!*

---

## 📖 Über das Projekt

**CashflowSimulator** ist eine leistungsstarke Desktop-Anwendung zur langfristigen Simulation von Vermögensentwicklung und Cashflows. Anders als einfache Zinseszins-Rechner zielt dieses Tool darauf ab, komplexe finanzielle Realitäten abzubilden.

Das Ziel ist eine detaillierte **Vorsorge- und Finanzplanung**, die echte steuerliche Gegebenheiten (Fokus: Deutschland) und Marktvolatilitäten berücksichtigt.

### Geplante Kernfunktionen

* **Detaillierte Vermögenssimulation:** Berücksichtigung von Aktien, Anleihen und ETFs.
* **Realistische Steuerlogik:** Implementierung des deutschen Steuerrechts (Kapitalertragsteuer, Vorabpauschale, Teilfreistellung, FIFO-Prinzip).
* **Marktsimulation:** Nutzung von historischen Daten oder Monte-Carlo-Simulationen (Volatilität, Drift), um Risiken sichtbar zu machen.
* **Cashflow-Events:** Einmalige oder wiederkehrende Einnahmen/Ausgaben (z.B. Gehalt, Renteneintritt, Hauskauf).
* **Privacy First:** Alle Daten werden lokal gespeichert. Keine Cloud, kein Tracking.

---

## 📸 Vorschau

*(Hier später Screenshots der Benutzeroberfläche einfügen, sobald die UI steht)*

---

## 🚀 Installation & Nutzung

### Für Anwender
Aktuell gibt es noch keine fertige Version zum Herunterladen. Sobald eine stabile Version verfügbar ist, wirst du sie hier unter [Releases](https://github.com/RalfHuesing/CashflowSimulator/releases) finden.

Die Anwendung wird plattformübergreifend für **Windows, macOS und Linux** verfügbar sein.

### Für Entwickler (Build from Source)
Wenn du dir den Code ansehen oder beim Aufbau helfen möchtest:

1.  **Voraussetzungen:**
    * [.NET 9 SDK](https://dotnet.microsoft.com/download)
    * Eine IDE (Visual Studio 2022, JetBrains Rider oder VS Code)

2.  **Repository klonen:**
    ```bash
    git clone [https://github.com/RalfHuesing/CashflowSimulator.git](https://github.com/RalfHuesing/CashflowSimulator.git)
    cd CashflowSimulator
    ```

3.  **Projekt bauen:**
    ```bash
    dotnet build CashflowSimulator.Desktop/CashflowSimulator.Desktop.csproj
    ```

4.  **Starten (Desktop):**
    ```bash
    cd CashflowSimulator.Desktop
    dotnet run
    ```

---

## 🛠 Technologie-Stack

Das Projekt setzt auf moderne .NET-Technologien und eine saubere Architektur (Clean Architecture / Schichtentrennung):

* **Core:** C# / .NET 9
* **UI Framework:** [Avalonia UI](https://avaloniaui.net/) (für Cross-Platform Desktop Support)
* **Architektur:**
    * `CashflowSimulator.Contracts`: Datendefinitionen, DTOs und fachliche Schnittstellen (z. B. `ISimulationRunner`, `IStockPriceService`).
    * `CashflowSimulator.Validation`: FluentValidation-Validatoren (Single Source of Truth für Regeln).
    * `CashflowSimulator.Engine`: Reine Rechenlogik: `SimulationRunner` (monatliche Pipeline) mit `ISimulationProcessor`-Kette (CashflowProcessor, GrowthProcessor, LiquidityProcessor, InflationProcessor); keine UI, keine I/O.
    * `CashflowSimulator.Infrastructure`: Persistenz (Projekte: JSON; Simulationsergebnisse: SQLite), Kursdaten (`DummyStockPriceProvider`), Implementierungen für Contracts-Interfaces.
    * **Hybrid-Persistenz:** Projekte werden als JSON gespeichert. Jeder Simulationslauf schreibt in eine **frische, flüchtige** SQLite-DB im Temp-Verzeichnis (`%Temp%\CashflowSimulator\`, OS-unabhängig). Vor jedem neuen Lauf werden nur eigene Dateien (`run_*.db`) in diesem Ordner gelöscht. Ein Run = ein Pfad (spätere Monte-Carlo-Erweiterung möglich).
    * `CashflowSimulator.Desktop`: Avalonia-UI, Composition Root, MVVM; Feature „Simulation starten“ und Anzeige der monatlichen Ergebnisse (Daten aus `IResultAnalysisService`/Repository).
    * `CashflowSimulator.Shared`: Gemeinsame Hilfen (derzeit Placeholder). Test-Projekte: Engine.Tests, Desktop.Tests, Validation.Tests.
* **Testing:** xUnit

---

## 🗺 Roadmap

Wir arbeiten aktuell an folgenden Meilensteinen:

- [x] Grundlegende Architektur & Datenmodelle (`Contracts`)
- [x] Lifecycle-Phasen-Modell (Steuer-/Strategie-Profile, Validierung, Defaults)
- [x] Monatliche Simulations-Pipeline (Engine: `SimulationRunner` mit Cashflow-, Growth-, Liquidity-, InflationProcessor)
- [x] Anzeige Simulationsergebnis (Desktop: „Simulation starten“, monatliche Ergebnisse)
- [x] Kursabfrage-Basis (`IStockPriceService`, `DummyStockPriceProvider`, Button „Kurs aktualisieren“ im Portfolio)
- [x] FIFO-Tranchen (Kauf-/Verkaufsbestand pro Asset: `AssetTrancheDto`, Engine FIFO bei Verkauf, UI-Anzeige und Validatoren)
- [ ] Steuer-Engine (Gewinn/Verlust aus FIFO, Vorabpauschale) – **geplant**
- [x] Validierungslogik für Eingaben
- [x] Grundaufbau der Benutzeroberfläche (Avalonia XAML, Shell, Navigation, Feature-Bereiche)
- [x] Persistierung (Speichern/Laden von Projekten)
- [x] SQLite-Ergebnis-Persistenz (ein Run = eine DB im Temp-Verzeichnis, RunId-basierte Anzeige)
- [ ] Erste lauffähige Beta-Version – **geplant**

---

## 🤝 Mitwirken

Beiträge sind willkommen! Da sich das Projekt noch im Aufbau befindet, öffne bitte zuerst ein **Issue**, bevor du einen Pull Request startest, um größere Änderungen zu besprechen.

---

## 📄 Lizenz

Dieses Projekt ist unter der [MIT Lizenz](LICENSE) veröffentlicht.