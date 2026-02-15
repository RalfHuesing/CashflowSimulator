# Stochastische Marktmodelle und Monte-Carlo-Engine

Diese Dokumentation beschreibt die **geplanten** mathematischen und technischen Konzepte für die generischen Marktdefinitionen (Stochastic Factors) und die Monte-Carlo-Simulation. Sie dient als Spezifikation für die spätere Engine-Implementierung in CashflowSimulator.Core bzw. einer dedizierten Engine.

---

## 1. Ökonomische Faktoren (Stochastic Factors)

### 1.1 Modelltypen

| Modell | Verwendung | Eigenschaft |
|--------|------------|-------------|
| **Geometric Brownian Motion (GBM)** | Aktien, Indizes (z. B. MSCI World), Rohstoffe (Gold) | Keine Mean-Reversion; Drift + Volatilität; log-normal verteilte Renditen. |
| **Ornstein-Uhlenbeck (OU)** | Zinsen, Inflation | Mittelwert-revertierend; Faktor kehrt zu einem langfristigen Niveau zurück. |

- **GBM:** \( dS_t = \mu S_t \, dt + \sigma S_t \, dW_t \). Diskrete Approximation (z. B. monatlich): \( S_{t+1} = S_t \cdot \exp\bigl((\mu - \sigma^2/2)\Delta t + \sigma\sqrt{\Delta t}\,Z\bigr) \) mit \( Z \sim N(0,1) \).
- **OU:** \( dX_t = \theta(\mu - X_t)\,dt + \sigma\,dW_t \). Diskrete Form (Euler-Maruyama oder exakte Lösung) mit Mean-Reversion-Speed \( \theta \); \( \theta = 0 \) entspricht effektiv Random Walk.

Die DTOs liefern \( \mu \) (ExpectedReturn), \( \sigma \) (Volatility), \( \theta \) (MeanReversionSpeed) und InitialValue; die Engine wählt je nach `StochasticModelType` die passende Update-Formel.

### 1.2 Korrelation zwischen Faktoren

Faktoren sind über **Pearson-Korrelationen** verknüpft. Die Einträge in `CorrelationEntryDto` definieren die paarweisen Koeffizienten \( \rho_{ij} \in [-1, 1] \). Die Engine bildet daraus die **Korrelationsmatrix** \( R \) (Diagonalen = 1) und erzeugt korrelierte Normalverteilungen via **Cholesky-Zerlegung**.

---

## 2. Cholesky-Zerlegung (Korrelierte Zufallsinnovationen)

### 2.1 Ziel

Pro Zeitschritt und pro Faktor wird eine Zufallsinnovation \( Z_i \sim N(0,1) \) benötigt. Sollen die Faktoren korreliert sein, dürfen die \( Z_i \) nicht unabhängig sein. Stattdessen erzeugen wir einen Vektor \( \mathbf{Z} \) unabhängiger \( N(0,1) \)-Zufallszahlen und transformieren ihn so, dass die resultierenden Innovationen die gewünschte Korrelationsmatrix \( R \) haben.

### 2.2 Vorgehen

1. **Korrelationsmatrix \( R \)** aus den DTOs aufbauen (Dimension = Anzahl EconomicFactors). Fehlende Einträge = 0 (unkorreliert); Diagonale = 1. Validierung: \( R \) muss **positiv definit** sein (alle Eigenwerte > 0).
2. **Cholesky-Zerlegung:** \( R = L L^\top \), wobei \( L \) untere Dreiecksmatrix ist.
3. **Innovationen:** Sei \( \mathbf{U} \in \mathbb{R}^n \) ein Vektor unabhängiger \( N(0,1) \)-Zufallszahlen. Dann hat \( \mathbf{Y} = L \mathbf{U} \) die gewünschte Korrelationsstruktur (Kovarianzmatrix proportional zu \( R \), bei Einheitsvarianz der \( U_i \) genau \( R \)).
4. Die \( Y_i \) werden in die jeweilige Faktor-Update-Formel (GBM oder OU) eingesetzt (anstelle einer einzelnen unabhängigen \( Z \)).

### 2.3 Hinweise für die Implementierung

- Nur wenn **mindestens zwei** Faktoren existieren und mindestens eine Korrelation \( \neq 0 \) ist, ist die Cholesky-Transformation nötig.
- Bei einem einzigen Faktor oder nur Nullen in den Korrelationen: unabhängige \( N(0,1) \) pro Faktor verwenden.
- Numerik: Cholesky schlägt fehl, wenn \( R \) nicht positiv definit ist (z. B. durch Rundungsfehler oder widersprüchliche Nutzereingaben). Dann: Fehler zurückgeben oder auf Nearest Positive Definite Matrix ausweichen (Projektentscheidung).

---

## 3. Seed-Replay-Pattern (Lightweight → Median → Heavyweight)

Um bei vielen Monte-Carlo-Iterationen effizient einen „repräsentativen“ Pfad (z. B. Median-Vermögen) zu analysieren, ohne alle Pfade dauerhaft zu speichern, wird folgendes **Seed-Replay-Pattern** empfohlen.

### 3.1 Ablauf (dreistufig)

1. **Lightweight-Run (erste Runde)**  
   - Simulation mit **gleichem Master-Seed** (`RandomSeed`) und **voller** `MonteCarloIterations`-Anzahl.  
   - Pro Iteration wird nur eine **aggregierte Kennzahl** pro Pfad berechnet (z. B. Endvermögen oder mittleres Vermögen über die Laufzeit).  
   - **Keine** Speicherung der kompletten Zeitreihen aller Pfade; nur die Liste der Kennzahlen (ein Wert pro Iteration).

2. **Median-Selektion**  
   - Aus der Liste der Kennzahlen wird der **Median** (oder ein anderes gewünschtes Perzentil) bestimmt.  
   - Es wird die **Iterations-Index** (bzw. der **Sub-Seed**) des Pfades identifiziert, der diesem Median am nächsten liegt (oder exakt den Median-Wert hat).  
   - Damit ist eindeutig festgelegt: „Pfad Nr. k repräsentiert den Median-Pfad.“

3. **Heavyweight-Replay (zweite Runde)**  
   - Simulation erneut mit **demselben Master-Seed** und derselben Anzahl Iterationen.  
   - Die Zufallsgeneratoren werden so initialisiert (z. B. pro Iteration ein abgeleiteter Seed aus Master-Seed + Iterationsindex), dass die **k-te Iteration bit-identisch** zum Lightweight-Run ist.  
   - Nur für **diese eine** Iteration k werden die **vollen Zeitreihen** (alle Faktoren, alle Vermögenswerte, alle Cashflows) berechnet und gespeichert.  
   - Ergebnis: Ein vollständiger „Median-Pfad“ ohne Speicherung aller Pfade.

### 3.2 Anforderungen an die Engine

- **Reproduzierbarkeit:** Bei festem `RandomSeed` und festem `MonteCarloIterations` muss die i-te Iteration immer dieselbe Zufallsfolge erzeugen (deterministisches Seeding pro Iteration, z. B. `Seed = RandomSeed + i` oder eine feste Ableitungsvorschrift).
- **Keine Abhängigkeit von Parallelisierungsreihenfolge:** Die i-te Iteration darf nicht davon abhängen, in welcher Reihenfolge andere Iterationen abgearbeitet werden (z. B. pro Iteration eigenen RNG mit abgeleitetem Seed verwenden).

### 3.3 Nutzen

- Speicher: Es werden nur eine Kennzahl pro Pfad (Lightweight) und ein vollständiger Pfad (Replay) gehalten.  
- Analyse: Der Nutzer kann den „typischen“ (Median-)Pfad detailliert ansehen (Charts, Export).  
- Debugging: Reproduzierbarkeit durch festen Seed und Replay desselben Index.

---

## 4. Datenquellen (DTOs)

- **EconomicFactorDto:** Id, Name, ModelType, ExpectedReturn, Volatility, MeanReversionSpeed, InitialValue.  
- **CorrelationEntryDto:** FactorIdA, FactorIdB, Correlation.  
- **SimulationProjectDto:** EconomicFactors, Correlations, RandomSeed, MonteCarloIterations.

Validierung (z. B. in CashflowSimulator.Validation): Faktor-IDs in Correlations müssen in EconomicFactors vorkommen; Correlation in \( [-1, 1] \); Korrelationsmatrix positiv definit; sinnvolle Grenzen für Volatility, MeanReversionSpeed, MonteCarloIterations.

---

*Stand: Spezifikation für die Engine-Implementierung; DTOs und Enums sind in CashflowSimulator.Contracts definiert.*
