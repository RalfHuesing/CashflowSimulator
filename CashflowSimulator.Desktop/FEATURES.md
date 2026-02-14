# Feature-Struktur (Vertical Slices)

Die Desktop-App ist **nach Domänen-Features** organisiert, nicht nach technischen Schichten (Views/ViewModels). Jedes Feature enthält alles, was es braucht: Views, ViewModels, ggf. Dialoge und feature-spezifische Hilfsklassen.

## Prinzip

- **Features/** = eine Ordnerhierarchie pro fachlichem Bereich.
- **Services/** = app-weite UI-Dienste (z. B. Dateidialog, HelpProvider), die von mehreren Features genutzt werden.
- **Common/** = Themes, wiederverwendbare Controls (z. B. FeatureLayoutView, FocusHelpBehavior).

So findest du alles zu „Eckdaten“ unter `Features/Eckdaten/`, alles zu „Meta“ unter `Features/Meta/`, alles zu „Cashflow“ später unter `Features/Cashflow/`.

## Aktuelle Struktur

```
CashflowSimulator.Desktop/
├── App.*, Program.cs, CompositionRoot.cs, MainWindow.*   # Root, Host
├── Common/Themes/, Common/Controls/, Common/Behaviors/    # Themes, FeatureLayoutView, FocusHelpBehavior
├── Services/                                              # App-weit (Dateidialog, HelpProvider, …)
└── Features/
    ├── Main/                                              # Shell: Sidebar (Logo, Navigation, Laden/Speichern), Content-Bereich (volle Höhe, keine Statusleiste)
    │   ├── MainShellView.axaml(.cs), MainShellViewModel.cs
    │   └── Navigation/NavigationView*, NavigationViewModel
    ├── Meta/                                              # Szenario-Stammdaten (MetaEditView*, MetaEditViewModel)
    ├── Eckdaten/                                          # Eckdaten (EckdatenView*, EckdatenViewModel)
    ├── Settings/                                          # Einstellungen (SettingsView*, SettingsViewModel)
    └── Cashflow/                                          # (später) Liste, Dialoge, …
```

## Namespaces

- `CashflowSimulator.Desktop.Services` – Dateidialog, HelpProvider etc.
- `CashflowSimulator.Desktop.Features.Main` – Shell, Navigation
- `CashflowSimulator.Desktop.Features.Main.Navigation` – Navigation-UI
- `CashflowSimulator.Desktop.Features.Meta` – Szenario-Metadaten
- `CashflowSimulator.Desktop.Features.Eckdaten` – Eckdaten
- `CashflowSimulator.Desktop.Features.Settings` – Einstellungen
- `CashflowSimulator.Desktop.Features.Cashflow` – (später)

## Pattern für neue Feature-Bereiche (wie Eckdaten, Einstellungen)

Damit neue Features gleich funktionieren wie Eckdaten/Einstellungen:

1. **Ordner** unter `Features/<Name>/` mit `*View.axaml`(.cs) und `*ViewModel.cs`.
2. **View:** UserControl mit **FeatureLayoutView** als Wurzel; Inhalt links (ScrollViewer im Template), rechts automatisch Info-Panel (Hilfe + Validierungsfehler).
3. **ViewModel:** Erbt von **ValidatingViewModelBase**; im Konstruktor **PageHelpKey** setzen (z. B. `"Eckdaten"`); **IHelpProvider** per Constructor Injection.
4. **Hilfe:** An jedem Eingabe-Control `FocusHelpBehavior.HelpKey="PropertyName"` und ggf. `FocusHelpBehavior.ErrorPropertyName="PropertyName"` setzen. FocusHelpBehavior wird einmalig in MainWindow initialisiert.
5. **Validierung:** Nur über FluentValidation; Fehler erscheinen **nur im rechten Info-Panel** (nicht unter den Controls). Keine Statusleiste.

Details siehe `.cursor/rules/main.md` (Abschnitt „Feature-Bereiche“) und `.cursor/rules/avalonia.md` („Feature-Layout und Hilfe“).

## Coding Guidelines (ViewModel/DTO)

- VM-Properties, die 1:1 DTO-Daten repräsentieren, müssen denselben Namen wie im DTO tragen (Ausnahme: andere Semantik, z. B. Alter vs. Datum).
- Validierungs-Mapping DTO → VM nur mit `nameof(DtoType.Property)` und `nameof(VmProperty)` – keine String-Literale. Siehe `.cursor/rules/main.md` (Validierung).

## Erweiterung

- Neues Feature = neuer Ordner unter `Features/<Name>/` mit Views, ViewModels, ggf. Dialogen; bei Bedarf gleiches Pattern wie Eckdaten (FeatureLayoutView, ValidatingViewModelBase, HelpKey).
- Gemeinsam genutzte Dialoge/Services bleiben in `Services/` oder werden bei Bedarf in ein gemeinsames Feature (z. B. `Features/Shared/`) ausgelagert.
