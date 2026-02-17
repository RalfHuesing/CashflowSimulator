# Projekt-TODO

Sammlung von To-dos und Ideen für den CashflowSimulator.

---

## Offen

- **Navigation:** Die Würfel (Cubes) und Expander in der Navigation – die Farbe ist unpassend (aktuell hellgrau, wirkt schlecht). Müssen überarbeitet werden.

- **Kurs-Abfrage (Engine):**
  - In der Engine eine einfache Schnittstelle zum Abfragen von Kursen einbauen.
  - Kurse laufen nicht zwingend über ISIN, sondern über **Symbole** → ggf. DTO anpassen/erweitern, damit Symbole gespeichert werden können.
  - Oberfläche: Button „Kurs aktualisieren“. Ablauf: Symbol suchen → wenn nicht vorhanden, Symbol ermitteln (über Kurs-Engine) → wenn Symbol da ist, Kurs abfragen.
  - **Provider-agnostisch** umsetzen: Die eigentliche Abfrage muss bewusst über austauschbare Provider laufen (verschiedene Anbieter möglich). Gegebenenfalls Websites parsen, wenn Provider wechseln oder etwas schiefgeht.

- **Allokationsprofile / Zielrichtung:**
  - Die neuen Allokationsprofile mit der Zielrichtung darunter sind grundsätzlich überarbeitungsbedürftig (unklar, ob das Konzept so bleiben soll).
  - Funktioniert derzeit nicht richtig: Buttons funktionieren nicht.
  - Die wählbare Layout-/Klasse wird nicht ansprechend dargestellt (erscheint z. B. als JSON-String).
  - Bezeichnungen wie „Front-Aim-Gear-Sum“ sind zu lang. Das Ganze muss überarbeitet werden.

- **Validierung (zu früh / zu strikt):**
  - Die Validierung schlägt sehr schnell an. Beim Herumklicken in der Anwendung (z. B. leere Eingabemaske, Klick ins Datumsfeld, dann woanders hin) erscheinen sofort Fehlermeldungen, obwohl noch nichts eingegeben wurde und kein Anlass für einen Fehler besteht.
  - Reproduzierbar tritt es beim „Herumklicken“ auf; exaktes Szenario nicht immer reproduzierbar. Validierungsverhalten überarbeiten (z. B. erst bei Blur/Verlassen des Felds oder bei explizitem Speichern validieren, nicht bei jedem Fokuswechsel).

- **Assets/ETFs – Layout, Transparenz, Scroll + zentrales Suchfeld (MasterDetailView):**
  - **Ausgangsproblem (daher dieser Eintrag):** Bei Vermögenswerten/ETFs: Beim Schieben des Sliders wird die Eingabemaske transparent, schiebt sich über die Liste, die Tabelle (Assets bzw. Transaktionen) lässt sich nicht scrollen. Bei anderen Bereichen (z. B. Cashflow, laufende Posten) scrollt alles normal – **Assets/Transaktionen sind die einzigen, die anders aufgebaut sind.**
  - **Ursache:** Nur in **PortfolioView** steht über der Liste ein **Suchfeld**; dafür wurde ein **StackPanel** (Suche + DataGrid) verwendet. StackPanel gibt der Liste keine feste Resthöhe, daher funktioniert das Scrollen der Tabelle nicht zuverlässig. Die anderen Views haben nur das DataGrid im Master – daher scrollt es dort.
  - **Ziel / Intention:** Das Suchfeld ist sinnvoll und soll **allen** Listen zugutekommen, die filtern wollen – aber **zentral** in der **MasterDetailView** umgesetzt werden, nicht pro View mit eigenem StackPanel. Dann haben wir ein einheitliches Layout (Suchzeile optional + Liste mit Resthöhe) und zuverlässiges Scrollen überall.
  - **Konkrete Schritte:**
    1. **Interface** (z. B. `IMasterDetailSearchable`): Property `SearchText` (string), optional `SearchWatermark` (string). ViewModels, die eine filterbare Liste anbieten, implementieren das Interface; die **Filterlogik** (was gefiltert wird) bleibt pro ViewModel.
    2. **MasterDetailView** anpassen: Master-Bereich als **Grid** mit Zeile 0 = Auto (Suchfeld), Zeile 1 = * (ContentPresenter für MasterContent). Suchfeld (TextBox) nur anzeigen, wenn DataContext das Interface implementiert; Bindung an `SearchText` (und ggf. Watermark). So bekommt die Liste immer die Resthöhe und scrollt korrekt.
    3. **PortfolioView:** Suchfeld und StackPanel entfernen; MasterContent = nur noch das DataGrid (wie bei den anderen Views). PortfolioViewModel implementiert bereits SearchText/Filter – bleibt so, nur die Anzeige kommt aus der MasterDetailView.
  - Damit sind Transparenz/Scroll (Layout) und das Suchfeld (zentral, opt-in) in einem Zug gelöst; alle weiteren Views können bei Bedarf das Interface implementieren und bekommen das Suchfeld ohne Layout-Probleme.

---

*Weitere Punkte einfach ergänzen.*
