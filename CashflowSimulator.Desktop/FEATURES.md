# Feature-Struktur (Vertical Slices)

Die Desktop-App ist **nach Domänen-Features** organisiert, nicht nach technischen Schichten (Views/ViewModels). Jedes Feature enthält alles, was es braucht: Views, ViewModels, ggf. Dialoge und feature-spezifische Hilfsklassen.

## Prinzip

- **Features/** = eine Ordnerhierarchie pro fachlichem Bereich.
- **Services/** = app-weite UI-Dienste (z. B. Dateidialog, HelpProvider), die von mehreren Features genutzt werden.
- **Common/** = Themes (DesignTokens, DataGridStyles, ButtonStyles, TypographyStyles, FormStyles, ErrorTrayStyles, FeatureLayoutViewStyle), wiederverwendbare Controls (FeatureLayoutView, MasterDetailView, FocusHelpBehavior).

So findest du alles zu „Eckdaten“ unter `Features/Eckdaten/`, alles zu „Meta“ unter `Features/Meta/`, alles zu „Cashflow“ später unter `Features/Cashflow/`.

## Aktuelle Struktur

```
CashflowSimulator.Desktop/
├── App.*, Program.cs, CompositionRoot.cs, MainWindow.*   # Root, Host
├── Common/Themes/*.axaml, Common/Controls/, Common/Behaviors/   # Styles (DesignTokens, DataGridStyles, ButtonStyles, …), FeatureLayoutView, MasterDetailView, FocusHelpBehavior
├── Services/                                              # App-weit (Dateidialog, HelpProvider, …)
└── Features/
    ├── Main/                                              # Shell: Sidebar (Logo, Navigation, Laden/Speichern), Content-Bereich (volle Höhe, keine Statusleiste)
    │   ├── MainShellView.axaml(.cs), MainShellViewModel.cs
    │   └── Navigation/NavigationView*, NavigationViewModel
    ├── Meta/                                              # Szenario-Stammdaten (MetaEditView*, MetaEditViewModel)
    ├── Eckdaten/                                          # Eckdaten (EckdatenView*, EckdatenViewModel)
    ├── Marktdaten/                                        # Stochastische Marktfaktoren (GBM, Mean Reversion); MasterDetailView
    ├── Korrelationen/                                     # Paarweise Korrelationen zwischen Faktoren; Matrix-Validierung (positiv definit)
    ├── CashflowStreams/                                   # Laufende Einnahmen/Ausgaben (MasterDetailView, optional Dynamisierung/Marktfaktor)
    ├── CashflowEvents/                                    # Geplante Einnahmen/Ausgaben (MasterDetailView, optional Dynamisierung/Marktfaktor)
    ├── Settings/                                          # Einstellungen (SettingsView*, SettingsViewModel)
    └── Cashflow/                                          # (später) ggf. weitere Cashflow-Themen
```

## Namespaces

- `CashflowSimulator.Desktop.Services` – Dateidialog, HelpProvider etc.
- `CashflowSimulator.Desktop.Features.Main` – Shell, Navigation
- `CashflowSimulator.Desktop.Features.Main.Navigation` – Navigation-UI
- `CashflowSimulator.Desktop.Features.Meta` – Szenario-Metadaten
- `CashflowSimulator.Desktop.Features.Eckdaten` – Eckdaten
- `CashflowSimulator.Desktop.Features.Settings` – Einstellungen
- `CashflowSimulator.Desktop.Features.Marktdaten` – Stochastische Faktoren (Marktdaten)
- `CashflowSimulator.Desktop.Features.Korrelationen` – Korrelationen zwischen Faktoren
- `CashflowSimulator.Desktop.Features.CashflowStreams` – Laufende Cashflows
- `CashflowSimulator.Desktop.Features.CashflowEvents` – Geplante Cashflow-Events
- `CashflowSimulator.Desktop.Features.Cashflow` – (später)

## Pattern für neue Feature-Bereiche (wie Eckdaten, Einstellungen)

Damit neue Features gleich funktionieren wie Eckdaten/Einstellungen:

1. **Ordner** unter `Features/<Name>/` mit `*View.axaml`(.cs) und `*ViewModel.cs`.
2. **View:** UserControl mit **FeatureLayoutView** als Wurzel; Inhalt links (ScrollViewer im Template), rechts automatisch Info-Panel (Hilfe + Validierungsfehler).
3. **ViewModel:** Erbt von **ValidatingViewModelBase**; **HelpKeyPrefix** implementieren (abstrakt, z. B. `=> "Eckdaten"`); im Konstruktor optional **PageHelpKey** setzen (default ist HelpKeyPrefix); **IHelpProvider** per Constructor Injection.
4. **Hilfe:** An jedem Eingabe-Control `FocusHelpBehavior.HelpKey="PropertyName"` setzen (kurzer Name; Lookup im HelpProvider erfolgt als `HelpKeyPrefix.PropertyName`). FocusHelpBehavior wird einmalig in MainWindow initialisiert.
5. **Validierung:** Nur über FluentValidation; Fehler erscheinen **nur im rechten Info-Panel** (nicht unter den Controls). Keine Statusleiste.

Details siehe `.cursor/rules/main.md` (Abschnitt „Feature-Bereiche“) und `.cursor/rules/avalonia.md` („Feature-Layout und Hilfe“).

## Coding Guidelines (ViewModel/DTO)

- VM-Properties, die 1:1 DTO-Daten repräsentieren, müssen denselben Namen wie im DTO tragen (Ausnahme: andere Semantik, z. B. Alter vs. Datum).
- Validierungs-Mapping DTO → VM nur mit `nameof(DtoType.Property)` und `nameof(VmProperty)` – keine String-Literale. Siehe `.cursor/rules/main.md` (Validierung).

## Marktdaten und Korrelationen

- **Marktdaten:** Stochastische Faktoren (z. B. Inflation VPI, Aktien Welt, Geldmarkt) mit Modelltyp (GBM oder Ornstein-Uhlenbeck) und Parametern (Drift, Volatilität, Mean-Reversion-Speed, Initialwert). Beim Löschen eines Faktors werden Referenzen in Streams/Events auf „Keine“ gesetzt.
- **Korrelationen:** Flache Liste der Korrelationseinträge (Faktor A ↔ Faktor B, Korrelation −1 bis 1). Die Gesamt-Korrelationsmatrix wird auf positive Definitheit (Cholesky) geprüft; bei Inkonsistenz erscheint ein Fehler im Info-Panel.
- **Dynamisierung:** In CashflowStreams und CashflowEvents kann pro Eintrag optional ein Marktfaktor zur Dynamisierung (z. B. Inflation) ausgewählt werden; „Keine“ = nominal.

## Lifecycle & Strategie (Datenmodell)

- **Lifecycle-Phasen:** Das Projekt enthält Lebensphasen (`LifecyclePhases`), die durch das Alter getriggert werden (z. B. Ansparphase ab aktuellem Alter, Rentenphase ab 67). Jede Phase verweist auf ein **Steuer-Profil** (`TaxProfiles`) und ein **Strategie-Profil** (`StrategyProfiles`).
- **Steuer-Profile:** Pro Phase z. B. Kapitalertragsteuer-Satz, Freibetrag, Einkommensteuer-Satz (nachgelagerte Besteuerung).
- **Strategie-Profile:** Pro Phase z. B. Liquiditätsreserve (Monate), Rebalancing-Schwelle, Lookahead (Monate) für geplante Events.
- **Asset-Allokation pro Phase:** Optional können in einer Phase Zielgewichtungen von Anlageklassen überschrieben werden (`AssetAllocationOverrides`).
- Die **Engine** (später) wählt pro Simulationsmonat die aktive Phase anhand des Alters und wendet das zugehörige Steuer- und Strategie-Profil an. Das Default-Projekt (DefaultProjectProvider) enthält zwei Phasen: „Anspar“ und „Rente“.

## Erweiterung

- Neues Feature = neuer Ordner unter `Features/<Name>/` mit Views, ViewModels, ggf. Dialogen; bei Bedarf gleiches Pattern wie Eckdaten (FeatureLayoutView, ValidatingViewModelBase, HelpKey). **Listen-Ansichten mit Bearbeitungsmaske** (z. B. CashflowStreams, CashflowEvents, Marktdaten, Korrelationen) nutzen **MasterDetailView** innerhalb von FeatureLayoutView und zentrale **DataGridStyles** – siehe `.cursor/rules/avalonia.md` (Master-Detail und Listen-Ansichten).
- Gemeinsam genutzte Dialoge/Services bleiben in `Services/` oder werden bei Bedarf in ein gemeinsames Feature (z. B. `Features/Shared/`) ausgelagert.
