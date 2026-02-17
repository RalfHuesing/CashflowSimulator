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

- **Assets/ETFs – Layout, Transparenz, Scroll:**
  - Bei ETFs/Assets in den Vermögenswerten: Slider zum Hin- und Herschieben. Über der Transaktionsliste wird es merkwürdig transparent, die Transaktionen lassen sich nicht mehr scrollen.
  - Die Eingabemaske unten ist transparent und schiebt sich über die Liste. Bei vielen Assets ist dort Scrollen möglich, bei Transaktionen nicht.
  - Bei anderen Bereichen (z. B. laufende Posten) tritt das nicht auf – dort scrollt alles normal. **Assets werden also anders behandelt als andere Listen.** Layout/Transparenz/Scroll-Verhalten angleichen bzw. beheben.

---

*Weitere Punkte einfach ergänzen.*
