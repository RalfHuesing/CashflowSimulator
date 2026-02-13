# Feature-Struktur (Vertical Slices)

Die Desktop-App ist **nach Domänen-Features** organisiert, nicht nach technischen Schichten (Views/ViewModels). Jedes Feature enthält alles, was es braucht: Views, ViewModels, ggf. Dialoge und feature-spezifische Hilfsklassen.

## Prinzip

- **Features/** = eine Ordnerhierarchie pro fachlichem Bereich.
- **Services/** = app-weite UI-Dienste (z. B. Dateidialog), die von mehreren Features genutzt werden.
- **Common/** = themes, wiederverwendbare Controls – unverändert.

So findest du alles zu „Meta“ unter `Features/Meta/`, alles zu „Cashflow“ später unter `Features/Cashflow/`.

## Aktuelle Struktur

```
CashflowSimulator.Desktop/
├── App.*, Program.cs, CompositionRoot.cs, MainWindow.*   # Root, Host
├── Common/Themes/, Common/Controls/                       # Unverändert
├── Services/                                              # App-weit, plattformunabhängige Abstraktionen
│   ├── IFileDialogService.cs                             # Öffnen/Speichern-Dialog (Avalonia = plattformunabhängig)
│   ├── AvaloniaFileDialogService.cs
│   └── IMetaEditDialogService.cs                         # Stammdaten-Dialog (Owner in MainWindow.OnLoaded)
└── Features/
    ├── Main/                                              # Shell: Banner, Navigation, Content-Bereich, Laden/Speichern
    │   ├── MainShellView.axaml(.cs)
    │   ├── MainShellViewModel.cs
    │   ├── Navigation/
    │   │   ├── NavigationView.axaml(.cs)                  # Links: Icon + Text
    │   │   └── NavigationViewModel.cs
    │   └── BottomBarView.axaml(.cs)                       # Unten: Laden, Speichern
    ├── Meta/                                              # Stammdaten / Szenario-Metadaten
    │   ├── MetaEditDialog.axaml(.cs)                      # Modal-Dialog zum Bearbeiten von MetaDto
    │   └── MetaEditDialogViewModel.cs
    └── Cashflow/                                          # (später) Liste, Dialoge, …
```

## Namespaces

- `CashflowSimulator.Desktop.Services` – Dateidialog etc.
- `CashflowSimulator.Desktop.Features.Main` – Shell, Navigation, BottomBar
- `CashflowSimulator.Desktop.Features.Main.Navigation` – Navigation-UI
- `CashflowSimulator.Desktop.Features.Meta` – Meta-Dialog
- `CashflowSimulator.Desktop.Features.Cashflow` – (später)

## Erweiterung

- Neues Feature = neuer Ordner unter `Features/<Name>/` mit Views, ViewModels, ggf. Dialogen.
- Gemeinsam genutzte Dialoge/Services bleiben in `Services/` oder werden bei Bedarf in ein gemeinsames Feature (z. B. `Features/Shared/`) ausgelagert.
