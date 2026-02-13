# CashflowSimulator.Desktop

Avalonia-Desktop-Client (.NET 9) des **Cashflow Simulators**. Einstiegspunkt und **Composition Root** der Anwendung. MVVM; keine Business-Logik in der UI – ViewModels wrappen DTOs aus Contracts, Fachlogik liegt in Engine/Infrastructure.

## Lösungskontext

- **Abhängigkeiten:** Dieses Projekt referenziert **CashflowSimulator.Contracts** (DTOs, Result, Interfaces). Optional später: Engine, Infrastructure.
- **Keine umgekehrten Abhängigkeiten:** Core/Infrastructure kennen die Desktop-App nicht. Siehe Solution-README bzw. `.cursor/rules/main.md` für die Gesamtarchitektur.

## Ordner- und Namespacestruktur (vereinbart)

```
CashflowSimulator.Desktop/
├── App.axaml, App.axaml.cs, Program.cs, MainWindow.*   # Root – Einstieg, Hauptfenster
├── CompositionRoot.cs                                  # Zugriff auf ServiceProvider (gesetzt in Program.Main)
├── Common/
│   ├── Themes/                                         # Zentrale Styles – keine hardcodierten Farben/Margins in Views
│   │   └── Default.axaml                               # Brushes, Thickness, ApplicationTitle, Basis-Styles
│   └── Controls/                                       # Wiederverwendbare UserControls (Namespace: CashflowSimulator.Desktop.Common.Controls)
├── Views/                                              # (geplant) Fenster, Dialoge, große Screens
├── ViewModels/                                          # (geplant) ViewModels
└── Services/                                            # (geplant) UI-Dienste (z. B. Dialoge öffnen)
```

- **Root-Namespace:** `CashflowSimulator.Desktop` für App, Program, MainWindow, CompositionRoot.
- **Common/Themes:** Reine XAML-Ressourcen (ResourceDictionary in Styles); in `App.axaml` per `StyleInclude Source="/Common/Themes/Default.axaml"` eingebunden.
- **Common/Controls:** UserControls mit Namespace `CashflowSimulator.Desktop.Common.Controls`.

## Technische Grundlagen

- **DI:** Microsoft.Extensions.DependencyInjection. Composition Root in `Program.Main` (vor `BuildAvaloniaApp()`); ServiceProvider über `CompositionRoot.Services`; MainWindow wird aus dem Container aufgelöst (`GetRequiredService<MainWindow>()`).
- **Logging:** Serilog; Konfiguration in `Program.Main`; in Libraries/Views nur `ILogger<T>` per Constructor Injection.
- **Constructor Injection:** Durchgängig; kein Service Locator.

## Styling-Regel

Keine hardcodierten Farben oder Margins in View-XAML. Alle Werte kommen aus **Common/Themes/Default.axaml** (Brushes, Thickness, Strings). Views und Controls referenzieren nur Ressourcen (`{StaticResource Key}`).

## Für AI / spätere Kontexteinordnung

- Dieses Projekt ist der **einzige Host** der Anwendung: hier laufen DI, Logging und Avalonia-Lifetime.
- Neue Fenster/Dialoge: als Views anlegen; bei Wiederverwendung von UI-Bausteinen: **Common/Controls** (UserControls).
- Neue Services (z. B. Engine, IStorageService): im Composition Root in `Program.Main` registrieren; dann in Fenster/ViewModels injizieren.
