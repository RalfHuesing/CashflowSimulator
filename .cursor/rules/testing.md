# Test-Struktur und Konventionen

## Wo welche Tests liegen

- **CashflowSimulator.Engine.Tests:** Unit-Tests für alle Engine-Typen (TaxContext, Processors, SimulationRunner). E2E/Integration der Simulation (z. B. SimulationIntegrationTests, Grand-Tour-Szenarien) gehören ebenfalls hier.
- **CashflowSimulator.Infrastructure.Tests:** Persistenz (JSON-Roundtrip, Speichern/Laden), Storage-Edge-Cases (z. B. DraftsFolderCleanup), Repository-Tests.
- **CashflowSimulator.Desktop.Tests:** UI-bezogene Tests, ViewModel-Tests; keine Engine-Domain-Logik (z. B. TaxContext-Tests liegen in Engine.Tests).
- **CashflowSimulator.Validation.Tests:** Ausschließlich Validatoren und Validierungsregeln.

## Naming

- Testmethoden: `MethodName_StateUnderTest_ExpectedBehavior` (z. B. `ApplyGeneralGain_PartialOffset_ReducesCarryforward`).

## Spezifikation für AI-Agenten

- Kalkulatoren und Prozessoren (z. B. InflationProcessor, GrowthProcessor) mit **`[Theory]` und `[InlineData]`** abdecken, damit Formeln und Randfälle als ausführbare Spezifikation dienen.
- In XML-Doc der Testmethode die Formel kurz beschreiben (z. B. „amount_after = amount_initial * e^(annualRate * months/12)“).

## Scope

- Es werden nur Tests für **implementierte** Features geschrieben. Nicht implementierte Features (z. B. Monte-Carlo) werden nicht getestet, bis sie umgesetzt sind.
