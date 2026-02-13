using Avalonia.Controls;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Desktop.Services;

namespace CashflowSimulator.Desktop.Features.Meta;

/// <summary>
/// Implementierung von <see cref="IMetaEditDialogService"/>: öffnet <see cref="MetaEditDialog"/> modal.
/// </summary>
public class MetaEditDialogService : IMetaEditDialogService
{
    private volatile Window? _owner;

    public void SetOwner(Window? owner) => _owner = owner;

    public async Task<MetaDto?> ShowEditAsync(MetaDto current, CancellationToken cancellationToken = default)
    {
        var owner = _owner;
        if (owner is null) return null;

        var dialog = new MetaEditDialog();
        var vm = new MetaEditDialogViewModel(current);
        dialog.SetViewModel(vm);

        await dialog.ShowDialog(owner).WaitAsync(cancellationToken).ConfigureAwait(true);
        return dialog.Result;
    }
}
