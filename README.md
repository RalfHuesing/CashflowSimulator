# Cashflow Simulator

## Überblick

Der Cashflow Simulator ist mehr als ein einfacher Renditerechner. Er bildet die finanzielle Lebenssituation eines Nutzers über Dekaden und verschiedene Lebensphasen hinweg ab. **Zielgruppe:** Deutsche mit langfristiger Finanzplanung (Anspar- und Rentenphase).

## Wozu? ? Kernfragen

Die Anwendung soll unter anderem beantworten:

- Kann ich mir das 3. Auto in 25 Jahren leisten?
- Wieviel Cash brauche ich in einem gegebenen Lebensabschnitt (Anspar- oder Entsparphase), um die nächsten N Jahre auch bei Börsencrashs abgesichert zu sein?
- Wieviel kann ich heute langfristig investieren, ohne mein definiertes Cash-Bucket zu gefährden?
- In welche Assets soll investiert werden ? inkl. Rebalancing-Strategie?
- Verschiedene Korrelationen und Strategien über die Lebensphasen simulieren; Verkäufe und Steuereffekte sind dabei zu berücksichtigen.

## Konzepte

### Lebensabschnitte

Es können Lebensabschnitte definiert werden (z.B. Ansparphase und Rentenphase). Pro Abschnitt: Beginn- und Endalter sowie ein **Lookahead-Zeitraum**. In diesem Zeitraum soll versucht werden, alle geplanten Ausgaben als Cash vorzuhalten. Typisch sind zwei Abschnitte: Ansparphase und Rentenphase.

### Cashflows

- **Laufend:** Einnahmen und Ausgaben.
- **Einmalig:** Geplante Einnahmen und Ausgaben (z.B. Auto in X Jahren).

Jedem Cashflow wird ein Lebensabschnitt zugeordnet; optional ein Von?Bis-Alter innerhalb dieses Abschnitts. In der Entsparphase können sich Werte und Verhalten ändern (z.B. andere Miete/Kosten) ? dafür können entsprechende Flows gepflegt werden.

**Unsicherheit:** Einige Events haben eine Toleranz (z.B. ?Auto in 10?12 Jahren?); das darauf folgende Event verschiebt sich entsprechend (z.B. +10 Jahre Abstand).

### Portfolio

- Ein Portfolio besteht aus **Assetallokationen** (z.B. MSCI World 60 %, Anleihen 40 %).
- Jede Allokation hat erwartete Rendite und Volatilität (für die Monte-Carlo-Simulation).
- Pro Asset können mehrere Wertpapiere existieren (z.B. verschiedene MSCI-World-ETFs über die Jahre). Nur ein Wertpapier pro Asset ist ?aktiv? und wird beim **Kauf** (Sparplan) verwendet.
- Beim **Verkauf** wird steueroptimiert entschieden (FIFO, Transaktionshistorie mit Kauf, Verkauf, Dividenden, Vorabpauschale). Getätigte Verkäufe fließen in die Akkumulation ein.

**Rebalancing:**

- **Per Sparplan:** Neues Geld wird in untergewichtete Assets gelenkt.
- **Aktiv:** Wenn die Ist-Allokation um mehr als **X %** von der Zielallokation abweicht, wird durch Verkauf/Kauf rebalanciert. Die Schwelle **X %** ist konfigurierbar.

### Unsicherheit und Simulation

- Zukünftige Wertschwankungen werden per **Monte-Carlo-Simulation** und Perzentil-Auswertung abgeschätzt.
- **Inflation** wird ebenfalls als Monte-Carlo modelliert (z.B. erwartete negative reale Rendite, Volatilität konfigurierbar, ggf. 0).
- Gleiches gilt für Gehalt und andere Cashflows sowie Wertpapiere. Die Schwankungsbereiche werden zentral definiert und den jeweiligen Assets bzw. Cashflows zugeordnet.

### Stammdaten und Steuern

- **Stammdaten:** u.a. Geburtsdatum (und Rentenalter).
- **Steuern:** Verkäufe über Dekaden (FIFO, Vorabpauschale etc.) werden in der Simulation berücksichtigt.

## Szenarien

Ein **Szenario** ist die vollständige Sammlung aller Eingaben für eine Planungsvariante (Person, Cashflows, Portfolio, Strategie, Optionen). Es können beliebig viele Szenarien angelegt werden; jedes wird typischerweise als eine Projektdatei (z.B. `Szenario.json`) gespeichert.

---

Technische Architektur, Datenmodell (`SimulationProjectDto`) und Datenfluss sind in **`.cursor/rules/main.md`** beschrieben.
