using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Common;
using CashflowSimulator.Contracts.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CashflowSimulator.Desktop.ViewModels;

/// <summary>
/// Generische Basisklasse für CRUD-ViewModels (Create, Read, Update, Delete).
/// Eliminiert duplizierten Code für Master-Detail-Formulare mit Liste und Bearbeitungsformular.
/// </summary>
/// <typeparam name="TDto">DTO-Typ, der <see cref="IIdentifiable"/> implementiert.</typeparam>
public abstract partial class CrudViewModelBase<TDto> : ValidatingViewModelBase
    where TDto : class, IIdentifiable
{
    private const int ValidationDebounceMs = 300;

    protected readonly ICurrentProjectService CurrentProjectService;
    protected bool IsLoading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private TDto? _selectedItem;

    /// <summary>
    /// Id des Eintrags im Bearbeitungsformular (null = Neu).
    /// </summary>
    [ObservableProperty]
    protected string? _editingId;

    /// <summary>
    /// Observable Collection der Items (Master-Liste).
    /// </summary>
    public ObservableCollection<TDto> Items { get; } = [];

    protected CrudViewModelBase(
        ICurrentProjectService currentProjectService,
        IHelpProvider? helpProvider = null)
        : base(helpProvider)
    {
        CurrentProjectService = currentProjectService;
        CurrentProjectService.ProjectChanged += OnProjectChanged;
        // RefreshItems() wird NICHT im Constructor aufgerufen, damit abgeleitete Klassen
        // ihre Initialisierung abschließen können (z. B. Dropdown-Optionen befüllen).
        // Abgeleitete Klassen müssen RefreshItems() oder ihre spezielle Variante selbst aufrufen.
    }

    partial void OnSelectedItemChanged(TDto? value)
    {
        if (value is null)
        {
            ClearForm();
            return;
        }
        IsLoading = true;
        try
        {
            EditingId = value.Id;
            MapDtoToForm(value);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Lädt die Items aus dem aktuellen Projekt.
    /// Abgeleitete Klassen implementieren hier den Zugriff auf die spezifische Liste
    /// (z. B. current.Streams, current.EconomicFactors).
    /// </summary>
    protected abstract IEnumerable<TDto> LoadItems();

    /// <summary>
    /// Schreibt die aktualisierte Liste zurück in den <see cref="ICurrentProjectService"/>.
    /// Abgeleitete Klassen rufen hier die passende Update-Methode auf
    /// (z. B. UpdateStreams, UpdateEconomicFactors).
    /// </summary>
    protected abstract void UpdateProject(IEnumerable<TDto> items);

    /// <summary>
    /// Erstellt ein DTO aus den aktuellen Formularfeldern.
    /// EditingId ist bereits gesetzt (oder null für neue Items).
    /// </summary>
    protected abstract TDto BuildDtoFromForm();

    /// <summary>
    /// Füllt die Formularfelder aus dem übergebenen DTO.
    /// Wird beim Auswählen eines Items in der Liste aufgerufen.
    /// </summary>
    protected abstract void MapDtoToForm(TDto dto);

    /// <summary>
    /// Leert das Formular und setzt Standardwerte (für "Neu").
    /// Soll auch <see cref="ValidatingViewModelBase.ClearValidationErrors"/> aufrufen.
    /// </summary>
    protected abstract void ClearFormCore();

    /// <summary>
    /// Validiert das übergebene DTO mit dem entsprechenden Validator aus <see cref="Validation.ValidationRunner"/>.
    /// Abgeleitete Klassen rufen hier ValidationRunner.Validate(dto) auf.
    /// </summary>
    protected abstract ValidationResult ValidateDto(TDto dto);

    /// <summary>
    /// Wird aufgerufen, wenn sich das aktuelle Projekt ändert.
    /// Standard-Verhalten: RefreshItems. Kann in abgeleiteten Klassen überschrieben werden
    /// (z. B. um auch Dropdown-Optionen zu aktualisieren).
    /// </summary>
    protected virtual void OnProjectChanged(object? sender, EventArgs e)
    {
        RefreshItems();
    }

    /// <summary>
    /// Lädt die Items neu aus dem Projekt und aktualisiert die Liste.
    /// </summary>
    protected void RefreshItems()
    {
        var items = LoadItems();
        Items.Clear();
        foreach (var item in items)
            Items.Add(item);
    }

    /// <summary>
    /// Leert das Formular; ruft ClearFormCore und löscht die EditingId und Validierungsfehler.
    /// </summary>
    private void ClearForm()
    {
        EditingId = null;
        ClearFormCore();
        ClearValidationErrors();
    }

    /// <summary>
    /// Plant eine verzögerte Validierung und optionales Auto-Save.
    /// Abgeleitete Klassen rufen dies bei Property-Änderungen auf.
    /// </summary>
    protected void ScheduleValidateAndSave()
    {
        if (IsLoading)
            return;
        ScheduleDebounced(ValidationDebounceMs, ValidateAndUpdateErrors);
    }

    private void ValidateAndUpdateErrors()
    {
        var dto = BuildDtoFromForm();
        var validationResult = ValidateDto(dto);
        SetValidationErrors(validationResult.Errors);
    }

    [RelayCommand]
    private void New()
    {
        SelectedItem = null;
        ClearForm();
        OnNewItemCreated();
    }

    /// <summary>
    /// Hook für abgeleitete Klassen, um nach dem Erstellen eines neuen Items
    /// zusätzliche Initialisierung durchzuführen (z. B. Standardwerte setzen).
    /// </summary>
    protected virtual void OnNewItemCreated()
    {
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        var current = CurrentProjectService.Current;
        if (current is null)
            return;

        var dto = BuildDtoFromForm();
        var validationResult = ValidateDto(dto);
        SetValidationErrors(validationResult.Errors);
        if (!validationResult.IsValid)
            return;

        var list = LoadItems().ToList();

        if (EditingId is null || !list.Any(x => x.Id == EditingId))
        {
            // Neues Item hinzufügen
            list.Add(dto);
            UpdateProject(list);
            RefreshItems();
            // Nach dem Hinzufügen Formular leeren für nächstes Item
            ClearForm();
        }
        else
        {
            // Bestehendes Item aktualisieren
            var idx = list.FindIndex(x => x.Id == EditingId);
            if (idx >= 0)
            {
                list[idx] = dto;
                UpdateProject(list);
                RefreshItems();
                // Nach Update: Item bleibt ausgewählt (SelectedItem wird durch RefreshItems aktualisiert)
                SelectedItem = Items.FirstOrDefault(x => x.Id == EditingId);
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private void Delete()
    {
        if (SelectedItem is null)
            return;
        var current = CurrentProjectService.Current;
        if (current is null)
            return;

        var deletedId = SelectedItem.Id;
        var list = LoadItems().Where(x => x.Id != deletedId).ToList();
        UpdateProject(list);

        // Hook für abgeleitete Klassen (z. B. Referenzen löschen)
        OnItemDeleted(deletedId);

        RefreshItems();
        SelectedItem = null;
        ClearForm();
    }

    /// <summary>
    /// Hook für abgeleitete Klassen, um nach dem Löschen eines Items
    /// zusätzliche Aufräumarbeiten durchzuführen (z. B. Referenzen in anderen Listen löschen).
    /// </summary>
    protected virtual void OnItemDeleted(string deletedId)
    {
    }

    private bool CanSave() => CurrentProjectService.Current is not null;
    private bool CanDelete() => SelectedItem is not null && CurrentProjectService.Current is not null;
}
