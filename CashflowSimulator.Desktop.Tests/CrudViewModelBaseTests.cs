using CashflowSimulator.Contracts.Common;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Services;
using CashflowSimulator.Desktop.ViewModels;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

/// <summary>
/// Unit-Tests für <see cref="CrudViewModelBase{TDto}"/> (generische CRUD-Logik).
/// </summary>
public sealed class CrudViewModelBaseTests
{
    /// <summary>Einfaches Test-DTO mit IIdentifiable.</summary>
    private record TestItemDto : IIdentifiable
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public int Value { get; init; }
    }

    /// <summary>Testbare Implementierung von CrudViewModelBase.</summary>
    private sealed class TestCrudViewModel : CrudViewModelBase<TestItemDto>
    {
        private readonly List<TestItemDto> _backingList = [];

        public TestCrudViewModel(ICurrentProjectService projectService)
            : base(projectService, null)
        {
            RefreshItems();
        }

        protected override string HelpKeyPrefix => "TestCrud";

        protected override IEnumerable<TestItemDto> LoadItems() => _backingList;

        protected override void UpdateProject(IEnumerable<TestItemDto> items)
        {
            _backingList.Clear();
            _backingList.AddRange(items);
        }

        protected override TestItemDto BuildDtoFromForm()
        {
            return new TestItemDto
            {
                Id = EditingId ?? Guid.NewGuid().ToString(),
                Name = TestName,
                Value = TestValue
            };
        }

        protected override void MapDtoToForm(TestItemDto dto)
        {
            TestName = dto.Name;
            TestValue = dto.Value;
        }

        protected override void ClearFormCore()
        {
            TestName = string.Empty;
            TestValue = 0;
        }

        protected override ValidationResult ValidateDto(TestItemDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return ValidationResult.Failure([new ValidationError(nameof(TestItemDto.Name), "Name ist erforderlich")]);
            return ValidationResult.Success();
        }

        // Public Properties für Tests
        public string TestName { get; set; } = string.Empty;
        public int TestValue { get; set; }
    }

    private static SimulationProjectDto CreateTestProject() => new()
    {
        Meta = new MetaDto { ScenarioName = "Test", CreatedAt = DateTimeOffset.UtcNow },
        Parameters = new SimulationParametersDto
        {
            SimulationStart = DateOnly.FromDateTime(DateTime.Today),
            SimulationEnd = DateOnly.FromDateTime(DateTime.Today.AddYears(30)),
            DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-40)),
            InitialLiquidCash = 0,
            CurrencyCode = "EUR"
        },
        UiSettings = new UiSettingsDto()
    };

    [Fact]
    public void Constructor_InitializesEmptyItems()
    {
        var projectService = new CurrentProjectService();
        projectService.SetCurrent(CreateTestProject());
        var vm = new TestCrudViewModel(projectService);

        Assert.Empty(vm.Items);
    }

    [Fact]
    public void NewCommand_ClearsFormAndSelectedItem()
    {
        var projectService = new CurrentProjectService();
        projectService.SetCurrent(CreateTestProject());
        var vm = new TestCrudViewModel(projectService);
        vm.TestName = "Existing";
        vm.TestValue = 42;

        vm.NewCommand.Execute(null);

        Assert.Null(vm.SelectedItem);
        Assert.Equal(string.Empty, vm.TestName);
        Assert.Equal(0, vm.TestValue);
    }

    [Fact]
    public void SaveCommand_WhenValid_AddsNewItem()
    {
        var projectService = new CurrentProjectService();
        projectService.SetCurrent(CreateTestProject());
        var vm = new TestCrudViewModel(projectService);
        vm.TestName = "Item 1";
        vm.TestValue = 100;

        vm.SaveCommand.Execute(null);

        Assert.Single(vm.Items);
        Assert.Equal("Item 1", vm.Items[0].Name);
        Assert.Equal(100, vm.Items[0].Value);
    }

    [Fact]
    public void SaveCommand_WhenInvalid_DoesNotSaveAndSetsErrors()
    {
        var projectService = new CurrentProjectService();
        projectService.SetCurrent(CreateTestProject());
        var vm = new TestCrudViewModel(projectService);
        vm.TestName = ""; // Ungültig

        vm.SaveCommand.Execute(null);

        Assert.Empty(vm.Items);
        Assert.True(vm.HasErrors);
    }

    [Fact]
    public void SaveCommand_WhenEditingExisting_UpdatesItem()
    {
        var projectService = new CurrentProjectService();
        projectService.SetCurrent(CreateTestProject());
        var vm = new TestCrudViewModel(projectService);
        
        // Erstelle erstes Item
        vm.TestName = "Original";
        vm.TestValue = 10;
        vm.SaveCommand.Execute(null);
        var itemId = vm.Items[0].Id;

        // Wähle das Item aus und bearbeite es
        vm.SelectedItem = vm.Items[0];
        vm.TestName = "Updated";
        vm.TestValue = 20;
        vm.SaveCommand.Execute(null);

        Assert.Single(vm.Items);
        Assert.Equal("Updated", vm.Items[0].Name);
        Assert.Equal(20, vm.Items[0].Value);
        Assert.Equal(itemId, vm.Items[0].Id); // ID bleibt gleich
    }

    [Fact]
    public void DeleteCommand_RemovesSelectedItem()
    {
        var projectService = new CurrentProjectService();
        projectService.SetCurrent(CreateTestProject());
        var vm = new TestCrudViewModel(projectService);
        
        // Erstelle Item
        vm.TestName = "To Delete";
        vm.TestValue = 99;
        vm.SaveCommand.Execute(null);
        Assert.Single(vm.Items);

        // Lösche Item
        vm.SelectedItem = vm.Items[0];
        vm.DeleteCommand.Execute(null);

        Assert.Empty(vm.Items);
        Assert.Null(vm.SelectedItem);
    }

    [Fact]
    public void SelectedItem_WhenSet_MapsToForm()
    {
        var projectService = new CurrentProjectService();
        projectService.SetCurrent(CreateTestProject());
        var vm = new TestCrudViewModel(projectService);
        
        // Erstelle Item
        vm.TestName = "Test Item";
        vm.TestValue = 42;
        vm.SaveCommand.Execute(null);

        // Leere Formular
        vm.NewCommand.Execute(null);
        Assert.Equal(string.Empty, vm.TestName);

        // Wähle Item aus
        vm.SelectedItem = vm.Items[0];

        Assert.Equal("Test Item", vm.TestName);
        Assert.Equal(42, vm.TestValue);
    }

    [Fact]
    public void CanExecuteSave_WhenNoProject_ReturnsFalse()
    {
        var projectService = new CurrentProjectService();
        var vm = new TestCrudViewModel(projectService);
        vm.TestName = "Valid Name";

        Assert.False(vm.SaveCommand.CanExecute(null));
    }

    [Fact]
    public void CanExecuteDelete_WhenNoSelection_ReturnsFalse()
    {
        var projectService = new CurrentProjectService();
        projectService.SetCurrent(CreateTestProject());
        var vm = new TestCrudViewModel(projectService);

        Assert.False(vm.DeleteCommand.CanExecute(null));
    }
}
