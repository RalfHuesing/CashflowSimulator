# Plan: Assets/ETFs – Transparenz, Slider, Scroll (Fix)

**Ausgangslage (aus TODO.md):** Beim Schieben des Sliders (MasterDetailView) wird die Eingabemaske unten transparent, schiebt sich über die Liste, und die Transaktionen/Liste lassen sich nicht scrollen. Bei anderen Bereichen (z. B. laufende Posten) tritt das nicht auf – Assets/Transaktionen sollen gleich behandelt werden wie andere Listen.

---

## Analyse (bereits geprüft)

- **FeatureLayoutView** (Style): Hauptbereich | **GridSplitter** | InfoPanel rechts. Alle Features nutzen das gleich.
- **MasterDetailView** (Portfolio + Transaktionen): **Liste oben** | **GridSplitter (horizontal)** | **Detail/Formular unten**. Wird in `PortfolioView` (Vermögenswerte) und `TransactionsView` (Transaktionen) genutzt.
- **Ursache vermutet:** Im `MasterDetailView` hat der Detail-Bereich (die `Border` in Zeile 21–24) **keinen gesetzten Background**. Dadurch wirkt die Eingabemaske transparent und kann über die Liste „rutschen“; zudem kann das Scroll-/Hit-Testing in die falsche Schicht gehen, sodass die Liste nicht mehr scrollbar erscheint.

Andere Features (z. B. laufende Posten) nutzen teils dasselbe MasterDetailView – wenn das Verhalten dort besser ist, kann es an anderem Content oder an der Höhenverteilung liegen. Ein einheitlicher, undurchsichtiger Detail-Bereich behebt die Transparenz und verbessert das Layoutverhalten.

---

## Maßnahmen (Reihenfolge)

1. **Detail-Bereich undurchsichtig machen**  
   In `CashflowSimulator.Desktop/Common/Controls/MasterDetailView.axaml`:  
   Auf der `Border` des Detail-Contents (Grid.Row="2") einen **expliziten Hintergrund** setzen, z. B.  
   `Background="{StaticResource ContentBackgroundBrush}"`  
   (oder `SurfaceBrush`, je nach gewünschtem Look).  
   → Verhindert Transparenz und „Überlagern“ der Liste.

2. **Master-Bereich explizit scrollbar machen (optional, falls nötig)**  
   Sicherstellen, dass der **Master-Content** (Liste) in einem **ScrollViewer** liegt oder die Liste selbst bei wenig Platz scrollt.  
   Aktuell: Master ist ein `ContentPresenter`; der eingebettete Content (z. B. `DataGrid`) scrollt von Haus aus. Wenn nach Schritt 1 das Scrollen in der Liste noch blockiert wird, den Master-Content in einen `ScrollViewer` wrappen, damit immer die Liste scrollt und nicht der äußere Container.

3. **Kurz testen**  
   - Vermögenswerte: Slider ziehen → Eingabemaske nicht mehr transparent, Liste bleibt sichtbar und scrollbar.  
   - Transaktionen: gleiches Verhalten.  
   - Andere Feature mit MasterDetailView (z. B. laufende Posten): unverändert funktionsfähig.

---

## Dateien

| Datei | Aktion |
|-------|--------|
| `CashflowSimulator.Desktop/Common/Controls/MasterDetailView.axaml` | Border Grid.Row="2": `Background` setzen (Schritt 1). Ggf. Master in ScrollViewer (Schritt 2). |

---

## Wenn es damit nicht reicht

- Prüfen, ob Portfolio/Transaktionen den Master-Content anders aufbauen (z. B. fehlendes `VerticalAlignment="Stretch"` am DataGrid) als andere Features.
- Prüfen, ob es zusätzliche Styles gibt, die nur für Portfolio/Transactions Opacity oder Background überschreiben.
- Ggf. in Avalonia nach bekannten Bugs zu GridSplitter + Transparenz/Scroll suchen.
