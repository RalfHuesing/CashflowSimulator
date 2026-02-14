# CashflowSimulator.Desktop

Avalonia-Desktop-Client (.NET 9) des **Cashflow Simulators**. Einstiegspunkt und **Composition Root** der Anwendung. MVVM; keine Business-Logik in der UI – ViewModels wrappen DTOs aus Contracts, Fachlogik liegt in Engine/Infrastructure.

## Lösungskontext

- **Abhängigkeiten:** Dieses Projekt referenziert **CashflowSimulator.Contracts** (DTOs, Result, Interfaces). Optional später: Engine, Infrastructure.
- **Keine umgekehrten Abhängigkeiten:** Core/Infrastructure kennen die Desktop-App nicht. Siehe Solution-README bzw. `.cursor/rules/main.md` für die Gesamtarchitektur.

## Ordner- und Namespacestruktur (Feature-basiert)

Die App ist nach **Domänen-Features** organisiert (Vertical Slices). Details siehe **FEATURES.md**.

```
CashflowSimulator.Desktop/
├── App.axaml, App.axaml.cs, Program.cs, MainWindow.*   # Root – Einstieg, Hauptfenster (hostet Shell)
├── CompositionRoot.cs                                  # Zugriff auf ServiceProvider (gesetzt in Program.Main)
├── Common/
│   ├── Themes/Default.axaml                            # Brushes, Thickness, ApplicationTitle – keine Hardcodes in Views
│   └── Controls/                                       # Wiederverwendbare UserControls
├── Services/                                            # App-weite UI-Dienste (plattformunabhängig)
│   ├── IFileDialogService.cs                           # Öffnen/Speichern (Avalonia StorageProvider)
│   └── AvaloniaFileDialogService.cs
└── Features/
    ├── Main/                                            # Shell: Banner, Navigation links, Content, Laden/Speichern unten
    │   ├── MainShellView*, MainShellViewModel
    │   ├── Navigation/NavigationView*, NavigationViewModel
    │   └── BottomBarView*
    ├── Meta/                                            # Stammdaten: MetaEditDialog
    └── Cashflow/                                        # (später) Liste, Dialoge
```

- **Root-Namespace:** `CashflowSimulator.Desktop` für App, Program, MainWindow, CompositionRoot.
- **Services:** `CashflowSimulator.Desktop.Services` – z. B. Dateidialog (Windows-unabhängig über Avalonia).
- **Features:** `CashflowSimulator.Desktop.Features.Main`, `.Features.Meta`, `.Features.Main.Navigation` usw.

## Technische Grundlagen

- **DI:** Microsoft.Extensions.DependencyInjection. Composition Root in `Program.Main` (vor `BuildAvaloniaApp()`); ServiceProvider über `CompositionRoot.Services`; MainWindow wird aus dem Container aufgelöst (`GetRequiredService<MainWindow>()`).
- **Logging:** Serilog; Konfiguration in `Program.Main` (Datei: `Logs/cashflow-{Date}.log`, Rolling pro Tag). In Libraries/ViewModels ausschließlich `ILogger<T>` per Constructor Injection. **Strukturiertes Logging:** Platzhalter wie `{Path}`, `{Error}` nutzen, keine String-Interpolation – siehe `.cursor/rules/main.md` (Abschnitt Logging). Dieses Muster bei allen neuen Log-Aufrufen beibehalten.
- **Constructor Injection:** Durchgängig; kein Service Locator.

## Styling-Regel

Keine hardcodierten Farben oder Margins in View-XAML. Alle Werte kommen aus **Common/Themes/Default.axaml** (Brushes, Thickness, Strings). Views und Controls referenzieren nur Ressourcen (`{StaticResource Key}`).

## Avalonia – Hinweise (nicht WPF)

- **XAML:** Root-Namespace `https://github.com/avaloniaui`; Controls aus Avalonia, keine `System.Windows.*`.
- **Lifetime:** Hauptfenster per `desktop.MainWindow = ...` setzen; Dialog-Owner ist `TopLevel`/`Visual` (z. B. MainWindow), nicht WPF-Window-Owner.
- **Ressourcen:** Über `StyleInclude` in App.axaml einbinden; StaticResource nur auf Ressourcen aus erreichbaren Styles zugreifen.
- **MVVM:** CommunityToolkit.Mvvm (`[RelayCommand]`, `[ObservableProperty]`, `ObservableObject`); Details in `.cursor/rules/main.md` und `.cursor/rules/avalonia.md`.
- **Validierung:** Nur über Validatoren (CashflowSimulator.Validation); Fehleranzeige im zentralen Meldungsbereich der Shell. Keine Validierung im XAML – siehe `.cursor/rules/main.md`.

## Für AI / spätere Kontexteinordnung

- Dieses Projekt ist der **einzige Host** der Anwendung: hier laufen DI, Logging und Avalonia-Lifetime.
- Neue Fenster/Dialoge: als Views anlegen; bei Wiederverwendung von UI-Bausteinen: **Common/Controls** (UserControls).
- Neue Services (z. B. Engine, IStorageService): im Composition Root in `Program.Main` registrieren; dann in Fenster/ViewModels injizieren.
