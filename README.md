# Cashflow Simulator

## Big Picture

Der Cashflow Simulator ist mehr als ein einfacher Rendite rechner.
Er soll die komplette finanzielle Lebenssituation eines Deutschen über Dekaden und verschiedene Lebensphasen hinweg abbilden können.


Es sollen Fragestellungen wie die folgenden beantwortet werden können:
- Kann ich mir das 3. Auto in 25 Jahren leisten
- Wieviel Cash brauche ich zu einem gegebenen Lebensabschnitt (Ansparphase / Entsparphase) um die nächsten N-Jahre auch mit Börsencrashs zu überstehen
- Wieviel muss KANN ich heute langfristig investieren damit mein definiertes Cash Bucket nicht gefährdet ist
- Wenn investiert wird - in welche Assets ("Rebalance per Sparplan")
- Es sollen verschiedene Korrelationen untereinander simuliert werden und die beste Strategie für die lebensphasen ermittelt werden. verkäufe und steuer effekte müssen natürlich auch beachtet werden

Daraus ergeben sich folgende Implikationen hinsichtlich Wert-Änderungen:
- In die Zukunft gerichtete Wertschwankungen können nur mit einer Montecarlo und perzentiler Auswertung abgeschätzt werden.
- Auch die Inflation ist eine Montecarlo-Simulation mit einer erwarteten negativ Rendite (-0.02) und einer Volatilität die auch 0 sein kann.
- Das bezieht sich auch auf das Gehalt und alle sonstigen Cashflows und insbesondere natürlich Wertepapiere.
- Diese Schwankungsbereiche (Simulationen) werden zentral definiert und können dem jeweiligen Asset oder Cashflow zugewiesen werden.
- Es gibt Feste Cashflow events und Cashflow events mit einer gewissen toleranz. Das auto in 10 jahren muss evtl. nicht genau in 10 jahren sein. 12 jahre würde auch gehen .. aber dann ist das darauf folgende auto erst mit einem gewissen abstand dran (+10 jahre). Es macht kein sinn jedes jahr ein neues auto zu kaufen.

Zielgruppe:
Deutsche

Notwendige Daten und Features
- Stammdaten: Geburtsdatum
- Steuern: Die Steuern insbesondere bei einem Dekaden später erfolgendem Verkauf müssen berücksichtigt werden
- Lebensabschnitte: Es können lebensabschnitte definiert werden. aus UI Sicht hinterlegt der benutzer hier das beginn und Ende Alter. Zusätzlich wird hier der Lookahead Zeitraum hinterlegt, in diesem zeitraum muss versucht werden alle notwendigen und geplanten ausgaben als cash vorhanden sein. Im Regelfall definiert man hier zwei Abschnitte: Ansparphase und Rentenphase
- Cashflows: Es gibt laufende Einnahmen, Laufende Ausgaben, geplante einmalige Einnahmen und geplante einmalige Ausgaben. Jedem Cashflow hinterlegt man zu welchem Lebensabschnitt er gehört. Zusätzlich kann man ein Von-Bis Alter hinterlegen - dieses ist im bereich des jeweiligen lebensabschnittes (also nicht über "67" wenn es die ansparphase ist).
Hier werden allerdings bestimmte flows dupliziert - wie beispielsweise die Miete. Aber richtigerweise ändert man sein verhalten in der Entsparphase und die Werte ändern sich auch
- Portfolio: Ein Portfolio setzt sich aus Assetallokations zusammen. Beispielsweise MSCI World (60) und Anleihen (40).
Jede allokation hat eine erwartete Rendite sowie Volatilität (Montecarlo). Pro Asset kann es beliebig viele Wertpapiere geben. Beispielsweise hat man im Laufe der Jahre verschiedene MSCI World ETFs bespart. Nur ein ETF kann "aktiv" sein - dieser wird verwendet wenn Gekauft wird. Beim VERKAUF wird sinnvoll entschieden was verkauft wird um steuern zu optimieren. Zusätzlich liegt hinter jedem Assets eine langjährige Transaktionshistorie mit Kauf, Verkauf, Dividenden und Steuern (Vorabpauschale). Wird verkauft wird dieses im FIFO durchgeführt und die steuer entsprechend berechnet.
Getätigte Verkäufe müssen bei der Akkumulation beachtet werden.

## Szenarios

Es gibt beliebig viele Szenarios.
Ein Szenario ist die vollständige Ansammlung aller notwendiger Informationen.

## Die neue Projekt-Architektur

Wir nutzen eine klare Trennung nach Verantwortlichkeiten, wobei die **Contracts** das Bindeglied für alles sind.

| Projekt | Verantwortung | Details |
| --- | --- | --- |
| **`.Contracts`** | **Die "Single Source of Truth"** | Enthält das `SimulationProjectDto`, alle Enums und Interfaces (z.B. `IPriceProvider`). |
| **`.Core`** | **Die Mathematik (Stateless)** | Beinhaltet die `SimulationEngine`, Wachstumsmodelle und Steuerlogik. Sie "rechnet" nur. |
| **`.Infrastructure`** | **Die Außenwelt** | Hier liegt die `StockPriceEngine` mit ihrem Datei-Cache und die Logik zum Laden/Speichern der Szenarien von der Festplatte. |
| **`.Desktop`** | **Die Benutzeroberfläche** | Avalonia UI Projekt. Die ViewModels "wrappen" die DTOs aus den Contracts und machen sie für den User editierbar. |

---

## Das SimulationProjectDto (Mentaler Blueprint)

Dieses Objekt ist deine "Projektdatei". Alles, was du zum Arbeiten brauchst, steckt hier drin. Wenn du dieses Objekt als JSON speicherst, hast du den kompletten Zustand konserviert.

### Grobe Struktur des Objekts:

* **`SimulationProjectDto` (Root)**
* `Id` & `ProjectName`: Metadaten zur Identifikation.
* **`ScenarioData`**: Die fachliche Welt (Was besitzt/plant der User?).
* `Person`: Geburtsjahr, Rentenalter.
* `Assets`: Liste aller Vermögenswerte (Name, ISIN, Vola, Erwartete Rendite).
* `Cashflows`: Einnahmen, Fixkosten und einmalige Events.
* `Strategy`: Rebalancing-Regeln und Steuer-Einstellungen.


* **`ExecutionOptions`**: Die "Regler" für die Engine.
* Anzahl Monte-Carlo-Iterationen, Simulationsdauer (Jahre), Inflation (An/Aus).


* **`UiState`**: Rein für die Desktop-App.
* Zuletzt gewählter Tab, Sortierung der Tabellen, Chart-Präferenzen, Fenstergröße.





---

## Der Datenfluss (Ohne Mapping-Hölle)

1. **Festplatte**: `Szenario.json` wird geladen  wird zu `SimulationProjectDto`.
2. **UI (Avalonia)**: Ein `MainViewModel` hält dieses DTO. Sub-ViewModels (z.B. `AssetViewModel`) greifen per **Backing Field** direkt auf die Properties des DTOs zu.
3. **Simulation**: Du klickst auf "Start"  Das **komplette** DTO wird an die `SimulationEngine` übergeben. Diese nimmt sich nur, was sie zum Rechnen braucht (`ScenarioData` & `ExecutionOptions`) und gibt ein `SimulationResultDto` zurück.
4. **Speichern**: Du änderst etwas im UI  Die Werte landen im DTO  Das DTO wird 1:1 als JSON auf die Platte geschrieben.

### Warum das für dich (und mich) gut ist:

* **Keine Redundanz**: Wir müssen nicht drei verschiedene Klassen für ein "Asset" pflegen.
* **Transparenz**: Wenn die Simulation komische Werte liefert, schauen wir in das eine DTO und sehen sofort, ob es an den User-Daten (`ScenarioData`) oder den Rechen-Parametern (`ExecutionOptions`) liegt.
* **Sprechende Namen**: Da wir neu anfangen, können wir die Properties sofort so nennen, dass sie selbsterklärend sind (z.B. `YearlyReturnExpectation` statt nur `Return`).

**Passt das so für dein mentales Modell? Wenn ja, soll ich dir als ersten Baustein die neue, saubere `SimulationProjectDto`-Struktur in C# zusammenschreiben?**
