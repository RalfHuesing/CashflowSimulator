using Avalonia.Controls;
using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Desktop.Features.Meta;

public partial class MetaEditDialog : Window
{
    public MetaEditDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Nach ShowDialog: gesetzt wenn der Nutzer OK geklickt hat, sonst null.
    /// </summary>
    public MetaDto? Result { get; private set; }

    public void SetViewModel(MetaEditDialogViewModel vm)
    {
        DataContext = vm;
        vm.CloseWithResult = r =>
        {
            Result = r;
            Close();
        };
    }
}
