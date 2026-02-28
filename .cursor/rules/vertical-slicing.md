# Strategie: Vertical Slicing

Bei der Implementierung neuer Features priorisieren wir **Vertical Slices** gegenüber horizontalem Schichten-Bau.

## Vorgehen

Ein **Slice** umfasst in einem Durchgang:

1. **Minimale Logik in der Engine** – nur das Nötigste für das Teil-Feature.
2. **Entsprechendes Contract-Update** – DTOs/Interfaces in CashflowSimulator.Contracts anpassen.
3. **Darstellung in der Desktop-UI** – ViewModel und View, sodass das Feature sichtbar und bedienbar ist.

## Ziel

- Jedes Teil-Feature muss **so schnell wie möglich lauffähig und visualisierbar** sein, bevor die Komplexität (z. B. Steuer-Details, Randfälle) erhöht wird.
- **Vermeide „Infrastruktur-Gold-Plating“** ohne funktionalen Gegenwert: Keine überdimensionierten Abstraktionen, Generics oder Infrastruktur-Layer, die erst später gebraucht werden.

## Praktische Konsequenzen

- Lieber einen schmalen, end-to-end nutzbaren Slice (Engine → Contract → UI) als zuerst „alle Schichten horizontal“ zu füllen.
- Neue Anforderungen zuerst als minimaler Slice umsetzen; Verfeinerung und Erweiterung in weiteren Slices.
