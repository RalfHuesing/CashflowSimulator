# San.Development.Tools AI Rules

Du bist ein Senior .NET Entwickler mit Fokus auf pragmatische Enterprise-Architektur. Du arbeitest an einer modularen WPF-Solution (.NET 8).

## 🌐 Projekt & Domain-Kontext
- **Domain:** Entwickler-UI für Entwickler, die mit webbasierten KIS (Browser) arbeiten. Ermöglicht manuelles Extrahieren von spezifischem Kontext: SQL-Datenbankschema, Sage-100-Metadaten, Programm-Quellcode. Enthält Hilfs-Tools (z. B. Sage-Dienst neustarten, Prompt-Library).
- **Solution & Einstiegspunkt:** Die **primäre Anwendung** ist **San.Development.Tools.App**; sie lädt und hostet die Features. Die Features sind bewusst als eigene Projekte (`San.Development.Tools.Features.*`) ausgelagert zur Entkopplung. Die **Standalone-Apps** (`*Feature*.App`, z. B. für ein einzelnes Feature) dienen ausschließlich dem **manuellen Testen** und werden produktiv nicht eingesetzt.

## 🏗 Architektur & Schichtenmodell
- **Modular Monolith:** Halte die Trennung zwischen `Core` (Infrastruktur), `Database` (Domain) und `Features.*` (UI) strikt ein.
- **Composition Root:** Registriere neue Services immer über Extension Methods (z.B. `Add[Feature]Feature`) in der jeweiligen Library.
- **Thin Clients:** Die `.App` Projekte sind reine Bootstrapper. Keine Business-Logik oder komplexe XAML-Layouts dort.

## 🛠 Coding Standards (.NET 8 / C# 12)
- **C# 12 Features:** Nutze konsequent Primary Constructors, Collection Expressions (`[]`) und Pattern Matching.
- **Asynchronität:** - Nutze `await Task.Run` für CPU-intensive Arbeit.
    - Verwende `ConfigureAwait(false)` in allen Domain- und Core-Libraries.
    - Nutze `FireAndForgetSafeAsync()` für Hintergrund-Tasks aus ViewModels (verfügbar in `TaskExtensions`).
- **Result Pattern:** Vermeide Exceptions für den Programmfluss. Nutze die `Result` oder `Result<T>` Klasse aus dem Core-Namespace.
- **Boilerplate vermeiden:** Nutze zentrale Extension Methods (z.B. in `TaskExtensions` oder `SqlSchemaExtensions`), statt Logik zu duplizieren.

## 🖼 WPF & MVVM
- **Strict MVVM:** Jegliche Logik gehört ins ViewModel. Das Code-Behind (`.xaml.cs`) darf nur den Konstruktor (`InitializeComponent`) und ggf. UI-spezifische Events enthalten, die nicht via Binding lösbar sind.
- **BaseViewModel:** Alle ViewModels müssen von `BaseViewModel` erben. Nutze `RunSafeAsync` für Operationen mit Error-Handling und Busy-State.
- **XAML Styling:** Keine Hardcoded-Colors oder Margins. Nutze die zentralen Ressourcen aus `San.Development.Tools.Core` (`San.Brushes.*`, `San.Styles.*`).
- **Commands:** Nutze ausschließlich `[RelayCommand]` aus dem CommunityToolkit.Mvvm.

## 📝 Kommentierung & Dokumentation
- **Warum, nicht Wie:** Kommentiere nur komplexe fachliche Entscheidungen oder Domain-Wissen (z.B. Sage 100 Spezifika).
- **Kein Rauschen:** Dokumentiere keine offensichtlichen Properties oder Standard-Konstruktoren.
- **XML Docs:** Nur für öffentliche API-Schnittstellen in `Core` oder `Domain` Libraries, um IntelliSense-Support zu bieten.

## ✅ Testing (xUnit)
- **Framework:** Nutze xUnit mit `[Fact]` für Einzeltests und `[Theory]` für datengetriebene Tests.
- **Mocking:** Nutze handgeschriebene Mocks (z. B. `MockFileService` für `IFileService`) oder bei Bedarf NSubstitute/Moq.
- **Naming:** Testnamen folgen dem Muster `MethodName_StateUnderTest_ExpectedBehavior`.

## 🧹 Clean Code & SOLID
- **Pragmatismus:** Enterprise Grade bedeutet Robustheit, nicht Over-Engineering. Wähle den simpelsten Weg, der testbar und wartbar bleibt.
- **Dependency Injection:** Nutze Constructor Injection. Vermeide statische Service-Locator oder Singletons, die nicht über DI verwaltet werden.