namespace CashflowSimulator.Desktop.ViewModels;

/// <summary>
/// Implementiert von ViewModels, die in einer MasterDetailView eine Suchleiste anzeigen möchten.
/// </summary>
public interface IMasterDetailSearchable
{
    string SearchText { get; set; }
    string? SearchWatermark { get; }
}
