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

## Validierung

- **Keine Validierung im XAML.** Grenzen und fachliche Regeln nur über die Validation-Schicht (CashflowSimulator.Validation); in Views keine `Minimum`/`Maximum` zur Regelung setzen.
- **Fehleranzeige:** Nur im rechten Info-Panel des Feature-Layouts (Block „VALIDIERUNGSFEHLER“). Die Standard-Anzeige unter den Controls ist in dieser App per Style deaktiviert (`DataValidationErrors` mit leerer `ErrorTemplate` in Common/Themes/ErrorTrayStyles.axaml).

## Feature-Layout und Hilfe

- **FeatureLayoutView** (Common/Controls): Wrapper für Feature-Seiten mit zweigeteiltem Layout – links scrollbarer Content, rechts festes Info-Panel (Hilfetext + Validierungsfehler). Neue Feature-Views wie Eckdaten/Einstellungen nutzen dieses Control als Wurzel.
- **FocusHelpBehavior** + **HelpKey:** Attached Property `FocusHelpBehavior.HelpKey` (und optional `ErrorPropertyName`) an Eingabe-Controls setzen (kurzer Property-Name). Bei GotFocus wird das ViewModel (ValidatingViewModelBase) mit dem HelpKey aktualisiert; Lookup im IHelpProvider erfolgt als **HelpKeyPrefix.PropertyName** (jedes Feature implementiert HelpKeyPrefix). Einmalige Initialisierung z. B. in MainWindow: `FocusHelpBehavior.Initialize(this)`.

## Datumsfelder (CalendarDatePicker)

- **Kultur:** In **Program.cs** wird zu App-Start **CultureInfo "de-DE"** für CurrentCulture und CurrentUICulture gesetzt, damit Datumseingaben (z. B. "15.02.2026") und Zahlenformate konsistent geparst werden.
- **DateOnly in ViewModels:** Datums-Properties in ViewModels sind **DateOnly?** (bzw. **DateOnly** wo fachlich erforderlich); DTOs nutzen ebenfalls DateOnly. Die Bindung an CalendarDatePicker erfolgt über den zentralen **DateOnlyConverter** (Common/Converters); in der View: `SelectedDate="{Binding StartDate, Converter={StaticResource DateOnlyConverter}}"`.
- **Deutsches Format:** An allen CalendarDatePicker **SelectedDateFormat="Custom"**, **CustomDateFormatString="dd.MM.yyyy"** und Watermark z. B. `"TT.MM.JJJJ"` setzen.

## Master-Detail und Listen-Ansichten

- **Listen-Ansichten mit Bearbeitungsmaske** (Liste oben, Formular unten): Es **muss** das **MasterDetailView** (Common/Controls) verwendet werden. Es wird **innerhalb** von FeatureLayoutView als Content eingesetzt; **MasterContent** = Liste/Tabelle (z. B. DataGrid), **DetailContent** = Formular; **NewCommand**, **SaveCommand**, **DeleteCommand** werden explizit an das Control gebunden. Der Detail-Bereich bleibt immer sichtbar (kein Ausblenden bei leerer Auswahl).
- **DataGrids:** Alle DataGrids in der App folgen den zentralen **DataGridStyles** (Common/Themes/DataGridStyles.axaml). Keine lokalen Overrides für Selection-, Header- oder Zeilen-Styles; Spaltenbreiten und spaltenspezifische Formatierung (z. B. rechtsbündige Beträge) bleiben in der Feature-View. Name/Bezeichnung-Spalte typisch `Width="*"` (responsive).
