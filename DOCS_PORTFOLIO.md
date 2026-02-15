# Portfolio und Assets – Markt vs. Besitz

Diese Dokumentation beschreibt das Zusammenspiel von **ökonomischen Faktoren (Markt)** und **Assets (Vermögenswerte)** im CashflowSimulator. Sie ist die fachliche Grundlage für die Datenstruktur in `SimulationProjectDto.Portfolio` und die spätere Engine-Logik (Wertentwicklung, Sparraten, Steuern).

---

## 1. Trennung von Markt und Besitz

### 1.1 EconomicFactor = Markt (Stochastik)

Ein **EconomicFactor** (z. B. "Aktien Welt", "Inflation", "Gold") definiert einen **simulierten Marktpfad**:

- Stochastisches Modell (z. B. GBM, Ornstein-Uhlenbeck)
- Parameter: Drift, Volatilität, ggf. Mean-Reversion, Startwert
- Die Engine erzeugt daraus über die Laufzeit der Simulation einen **Preis- bzw. Indexpfad**

Ein Faktor hat **keine** Stückzahl, keine Transaktionen und keine steuerlichen Attribute. Er ist die reine „Renditequelle“.

### 1.2 Asset = Besitz (Holding)

Ein **Asset** ist ein **konkreter Vermögenswert**, den der Nutzer hält (z. B. "Vanguard FTSE All-World", "iShares MSCI World"):

- **Verknüpfung zum Markt:** Über `EconomicFactorId` wird genau ein EconomicFactor referenziert. Die Wertentwicklung dieses Assets folgt dem simulierten Pfad dieses Faktors.
- **Eigene Bestandsdaten:** Stückzahl (`CurrentQuantity`), Transaktionshistorie (`Transactions`), steuerliche Einordnung (`TaxType`), ISIN, Name.
- **Status:** `IsActiveSavingsInstrument` kennzeichnet, ob neue Sparraten in dieses Asset fließen (siehe Abschnitt 2).

**Wichtig:** Mehrere Assets können **dieselbe** `EconomicFactorId` haben. Dann entwickeln sie sich alle nach dem **gleichen** Marktpfad, haben aber getrennte Bestände und Historie.

---

## 2. Multi-Asset-Historie: aktive vs. passive ETFs auf denselben Index

### 2.1 Szenario

Ein Nutzer kann über die Jahre **verschiedene** ETFs auf denselben Index angesammelt haben, z. B.:

- ETF A: alter MSCI-World-ETF (nicht mehr bespart, nur noch Bestand)
- ETF B: neuer MSCI-World-ETF (aktuelles Sparplan-Instrument)
- ETF C: weiterer MSCI-World-ETF (z. B. anderer Anbieter, einmalig gekauft)

Alle drei haben dieselbe **EconomicFactorId** (z. B. "MSCI_World"). In der Simulation:

- **Alle drei** wachsen kurstechnisch mit demselben simulierten Indexpfad.
- **Nur einer** erhält neue Sparraten: der, bei dem `IsActiveSavingsInstrument = true` ist (typischerweise ETF B).

Die anderen sind „tote“ Bestände: Sie partizipieren an der Wertentwicklung des Faktors, bekommen aber keine neuen Käufe zugeordnet.

### 2.2 Modellierung im DTO

| Aspekt | Umsetzung |
|--------|-----------|
| Gleiche Wertentwicklung | Alle drei Assets: `EconomicFactorId = "MSCI_World"` (oder die jeweilige Faktor-ID). |
| Nur einer wird bespart | Genau ein Asset (z. B. ETF B): `IsActiveSavingsInstrument = true`; A und C: `false`. |
| Getrennte Bestände | Jedes Asset hat eigene `CurrentQuantity`, `Transactions`, `Id`, `Name`, `Isin`. |
| Steuer/FIFO | Pro Asset eigene Transaktionshistorie für FIFO bei Verkäufen und Vorabpauschale. |

Damit ist die DTO-Struktur in der Lage, beliebig viele MSCI-World-ETFs (oder andere Fonds auf demselben Faktor) abzubilden, von denen nur einer das aktive Sparziel ist.

---

## 3. Steuerliche Relevanz der Assets

Assets tragen Attribute für die **deutsche Besteuerung**:

- **TaxType:** z. B. Aktienfonds (Teilfreistellung 30 %), Mischfonds, Anleihenfonds, None (voll steuerpflichtig). Wird für Veräußerungsgewinne und Ausschüttungen genutzt.
- **Transaktionshistorie:** Für eine exakte **FIFO-Steuerberechnung** bei Verkäufen müssen Kaufdaten und -mengen vorliegen (`TransactionDto` mit Typ `Buy`/`Sell`). Jede Transaktion hat eine eindeutige **Id**; die Desktop-App nutzt diese für robustes Bearbeiten und Löschen (unabhängig von Listenreihenfolge/Sortierung). Die Engine kann anhand der Id bzw. der Historie realisierte Gewinne/Verluste und Kapitalertragsteuer ableiten.
- **Vorabpauschale / Ausschüttung:** Transaktionstypen `TaxPrepayment` und `Dividend` in der Historie unterstützen die Nachbildung von Besteuerung und Cashflows.

Die detaillierte Steuerlogik (FIFO, Teilfreistellung, Verrechnung) liegt in der Engine (Core); die Contracts liefern die dafür notwendigen Daten.

---

## 4. Datenstruktur-Überblick

| DTO | Rolle |
|-----|--------|
| **EconomicFactorDto** | Markt: Id, Name, Modell, Drift, Volatilität, Startwert. Keine Stückzahl, keine Transaktionen. |
| **AssetDto** | Besitz: Id, Name, ISIN, **EconomicFactorId**, IsActiveSavingsInstrument, TaxType, CurrentQuantity, CurrentValue, Transactions. |
| **TransactionDto** | Einzelne Buchung: **Id** (eindeutig, Guid-String), Datum, Typ (Buy/Sell/Dividend/TaxPrepayment), Menge, Preis, Gesamtbetrag, Steueranteil. Die Id ermöglicht robustes Update/Löschen in der UI und eine eindeutige Zuordnung für die Engine (z. B. FIFO). |
| **PortfolioDto** | Container: Liste aller Assets, optional Strategy (Rebalancing etc., später). |

`SimulationProjectDto` enthält sowohl `EconomicFactors` als auch `Portfolio` (mit `Portfolio.Assets`). Referenzierung: Jedes `AssetDto.EconomicFactorId` muss auf eine `EconomicFactorDto.Id` aus `SimulationProjectDto.EconomicFactors` verweisen (Validierung in CashflowSimulator.Validation).
