# CashflowSimulator

![License](https://img.shields.io/github/license/DEIN_USERNAME/CashflowSimulator)
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
Aktuell gibt es noch keine fertige Version zum Herunterladen. Sobald eine stabile Version verfügbar ist, wirst du sie hier unter [Releases](https://github.com/DEIN_USERNAME/CashflowSimulator/releases) finden.

Die Anwendung wird plattformübergreifend für **Windows, macOS und Linux** verfügbar sein.

### Für Entwickler (Build from Source)
Wenn du dir den Code ansehen oder beim Aufbau helfen möchtest:

1.  **Voraussetzungen:**
    * [.NET 9 SDK](https://dotnet.microsoft.com/download)
    * Eine IDE (Visual Studio 2022, JetBrains Rider oder VS Code)

2.  **Repository klonen:**
    ```bash
    git clone [https://github.com/DEIN_USERNAME/CashflowSimulator.git](https://github.com/DEIN_USERNAME/CashflowSimulator.git)
    cd CashflowSimulator
    ```

3.  **Projekt bauen:**
    ```bash
    dotnet build
    ```

4.  **Starten (Desktop):**
    ```bash
    cd CashflowSimulator.Desktop
    dotnet run
    ```

---

## 🛠 Technologie-Stack

Das Projekt setzt auf moderne .NET-Technologien und eine saubere Architektur (Clean Architecture):

* **Core:** C# / .NET 9
* **UI Framework:** [Avalonia UI](https://avaloniaui.net/) (für Cross-Platform Desktop Support)
* **Architektur:**
    * `Focus.Engine`: Die reine Rechenlogik (Steuern, Simulation).
    * `Focus.Contracts`: Datendefinitionen und Schnittstellen.
    * `Focus.Desktop`: Die MVVM-basierte Benutzeroberfläche.
* **Testing:** xUnit

---

## 🗺 Roadmap

Wir arbeiten aktuell an folgenden Meilensteinen:

- [x] Grundlegende Architektur & Datenmodelle (`Contracts`)
- [ ] Implementierung der Steuer-Engine (FIFO, Vorabpauschale)
- [ ] Validierungslogik für Eingaben
- [ ] Aufbau der Benutzeroberfläche (Avalonia XAML)
- [ ] Persistierung (Speichern/Laden von Projekten)
- [ ] Erste lauffähige Beta-Version

---

## 🤝 Mitwirken

Beiträge sind willkommen! Da sich das Projekt noch im Aufbau befindet, öffne bitte zuerst ein **Issue**, bevor du einen Pull Request startest, um größere Änderungen zu besprechen.

---

## 📄 Lizenz

Dieses Projekt ist unter der [MIT Lizenz](LICENSE) veröffentlicht.