# Avalonia – Spezifika (nicht WPF)

Diese Regeln gelten für die **CashflowSimulator.Desktop**-Avalonia-App. Nicht mit WPF verwechseln.

## XAML und Namespaces

- **Root-Namespace:** `xmlns="https://github.com/avaloniaui"` (nicht `http://schemas.microsoft.com/winfx/2006/xaml/presentation`).
- **Controls:** Aus `Avalonia.Controls.*`; keine `System.Windows.*`-Controls.

## Styling und Ressourcen

- **App-Styles:** In `Application.Styles` mit `<StyleInclude Source="..."/>` einbinden; Ressourcen in `<Styles.Resources><ResourceDictionary>...</ResourceDictionary></Styles.Resources>`.
- **Selektoren:** Avalonia-eigene Syntax, z. B. `Style Selector="Button"`; keine WPF-ResourceDictionary-Keys in Selectors.
- **StaticResource/DynamicResource:** Ressourcen müssen in erreichbaren Styles/ResourceDictionaries liegen (z. B. über StyleInclude in App.axaml), sonst Laufzeitfehler.

## Binding

- **Compiled Bindings:** Im Projekt ist `AvaloniaUseCompiledBindingsByDefault` gesetzt; neue Bindings nutzen Compiled Bindings, wo möglich (bessere Performance und Typprüfung).
- **ICommand:** Avalonia nutzt `System.Windows.Input.ICommand`; CommunityToolkit.Mvvm (RelayCommand/IAsyncRelayCommand) ist kompatibel.

## Lifetime und Fenster

- **Desktop-Lifetime:** `IClassicDesktopStyleApplicationLifetime`; Hauptfenster per `desktop.MainWindow = ...` setzen (nicht „Show()“ wie bei WPF-App-Start).
- **Dialog-Owner:** Owner für File-/Dialog-Dienste ist `TopLevel`/`Visual` (z. B. MainWindow), nicht das WPF-`Window`-Owner-Pattern; typisch `SetOwner(this)` vom MainWindow aus aufrufen.

## Code-Behind und Struktur

- Keine Business-Logik im Code-Behind; nur `InitializeComponent` und UI-Events, die nicht per Binding abbildbar sind.
- UserControls und ContentControls: Avalonia-spezifische Basisklassen verwenden.
