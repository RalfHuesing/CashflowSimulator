using CashflowSimulator.Contracts.Common;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Desktop.ViewModels;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

/// <summary>
/// Unit-Tests für <see cref="ValidatingViewModelBase"/> (INotifyDataErrorInfo, Mapping, FormLevelErrors).
/// </summary>
public sealed class ValidatingViewModelBaseTests
{
    private sealed class TestableValidatingViewModel : ValidatingViewModelBase
    {
        protected override string HelpKeyPrefix => "Test";

        public void SetErrorsPublic(IReadOnlyList<ValidationError> errors, IReadOnlyDictionary<string, string>? map = null) =>
            SetValidationErrors(errors, map);

        public void ClearPublic() => ClearValidationErrors();
    }

    [Fact]
    public void HasErrors_WhenNoErrors_ReturnsFalse()
    {
        var vm = new TestableValidatingViewModel();
        Assert.False(vm.HasErrors);
    }

    [Fact]
    public void SetValidationErrors_WithOneError_SetsHasErrorsAndGetErrors()
    {
        var vm = new TestableValidatingViewModel();
        vm.SetErrorsPublic([new ValidationError("DateOfBirth", "Geburtsdatum muss angegeben werden.")]);

        Assert.True(vm.HasErrors);
        var errors = vm.GetErrors("DateOfBirth").Cast<string>().ToList();
        Assert.Single(errors);
        Assert.Equal("Geburtsdatum muss angegeben werden.", errors[0]);
    }

    [Fact]
    public void SetValidationErrors_WithDtoToVmMapping_MapsPropertyName()
    {
        var vm = new TestableValidatingViewModel();
        var map = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { nameof(SimulationParametersDto.SimulationEnd), "LifeExpectancy" }
        };
        vm.SetErrorsPublic([new ValidationError(nameof(SimulationParametersDto.SimulationEnd), "Ungültig.")], map);

        Assert.Empty(vm.GetErrors(nameof(SimulationParametersDto.SimulationEnd)).Cast<string>().ToList());
        var vmErrors = vm.GetErrors("LifeExpectancy").Cast<string>().ToList();
        Assert.Single(vmErrors);
        Assert.Equal("Ungültig.", vmErrors[0]);
    }

    [Fact]
    public void SetValidationErrors_WithEmptyPropertyName_GoesToFormLevelErrors()
    {
        var vm = new TestableValidatingViewModel();
        vm.SetErrorsPublic([new ValidationError("", "Renteneintrittsalter muss zwischen 50 und 75 Jahren liegen.")]);

        Assert.True(vm.HasFormLevelErrors);
        var formErrors = vm.FormLevelErrors.ToList();
        Assert.Single(formErrors);
        Assert.Equal("Renteneintrittsalter muss zwischen 50 und 75 Jahren liegen.", formErrors[0]);
    }

    [Fact]
    public void ClearValidationErrors_RemovesAllErrors()
    {
        var vm = new TestableValidatingViewModel();
        vm.SetErrorsPublic([new ValidationError("X", "Fehler")]);
        Assert.True(vm.HasErrors);

        vm.ClearPublic();
        Assert.False(vm.HasErrors);
        Assert.Empty(vm.GetErrors("X").Cast<string>().ToList());
    }

    [Fact]
    public void SetValidationErrors_ReplacesPreviousErrorsForSameProperty()
    {
        var vm = new TestableValidatingViewModel();
        vm.SetErrorsPublic([new ValidationError("A", "Erster")]);
        vm.SetErrorsPublic([new ValidationError("A", "Zweiter")]);

        var errors = vm.GetErrors("A").Cast<string>().ToList();
        Assert.Single(errors);
        Assert.Equal("Zweiter", errors[0]);
    }
}
