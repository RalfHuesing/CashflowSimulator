using System.Collections;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using CashflowSimulator.Desktop.ViewModels;

namespace CashflowSimulator.Desktop.Common.Behaviors;

/// <summary>
/// Attached properties und zentrale Logik für Fokus-Hilfe und Fehler-Anzeige.
/// HelpKey: wird bei GotFocus auf dem ViewModel (ValidatingViewModelBase) als ActiveHelpKey gesetzt.
/// ErrorPropertyName: GetErrors(ErrorPropertyName) steuert die CSS-Klasse "has-error" am Control.
/// </summary>
public static class FocusHelpBehavior
{
    private static Window? _rootWindow;
    private static readonly Dictionary<Control, (EventHandler<DataErrorsChangedEventArgs> Handler, string PropertyName)> _errorSubscriptions = new();

    public static readonly AttachedProperty<string?> HelpKeyProperty =
        AvaloniaProperty.RegisterAttached<Control, string?>("HelpKey", typeof(FocusHelpBehavior), defaultBindingMode: Avalonia.Data.BindingMode.OneWay);

    public static readonly AttachedProperty<string?> ErrorPropertyNameProperty =
        AvaloniaProperty.RegisterAttached<Control, string?>("ErrorPropertyName", typeof(FocusHelpBehavior), defaultBindingMode: Avalonia.Data.BindingMode.OneWay);

    public static string? GetHelpKey(AvaloniaObject obj) => obj.GetValue(HelpKeyProperty);
    public static void SetHelpKey(AvaloniaObject obj, string? value) => obj.SetValue(HelpKeyProperty, value);

    public static string? GetErrorPropertyName(AvaloniaObject obj) => obj.GetValue(ErrorPropertyNameProperty);
    public static void SetErrorPropertyName(AvaloniaObject obj, string? value) => obj.SetValue(ErrorPropertyNameProperty, value);

    /// <summary>
    /// Initialisierung: an das Fenster anmelden, damit GotFocus gebubbelt verarbeitet wird.
    /// Wird von <see cref="MainWindow"/> beim Laden aufgerufen.
    /// </summary>
    public static void Initialize(Window window)
    {
        if (_rootWindow == window)
            return;
        _rootWindow?.RemoveHandler(InputElement.GotFocusEvent, OnGlobalGotFocus);
        _rootWindow = window;
        window.AddHandler(InputElement.GotFocusEvent, OnGlobalGotFocus, Avalonia.Interactivity.RoutingStrategies.Bubble);
    }

    private static void OnGlobalGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (e.Source is not Control control)
            return;

        var helpKey = GetHelpKey(control);
        if (!string.IsNullOrEmpty(helpKey))
        {
            if (control.DataContext is ValidatingViewModelBase vm)
                vm.ActiveHelpKey = helpKey;
        }

        var errorProp = GetErrorPropertyName(control);
        if (!string.IsNullOrEmpty(errorProp) && control.DataContext is INotifyDataErrorInfo notifyDataErrorInfo)
            EnsureErrorSubscription(control, notifyDataErrorInfo, errorProp);
    }

    private static void EnsureErrorSubscription(Control control, INotifyDataErrorInfo vm, string propertyName)
    {
        if (_errorSubscriptions.TryGetValue(control, out var existing))
        {
            if (existing.PropertyName == propertyName)
                return;
            vm.ErrorsChanged -= existing.Handler;
            _errorSubscriptions.Remove(control);
        }

        void OnErrorsChanged(object? _, DataErrorsChangedEventArgs args)
        {
            if (args.PropertyName != propertyName)
                return;
            UpdateHasErrorClass(control, vm, propertyName);
        }

        vm.ErrorsChanged += OnErrorsChanged;
        _errorSubscriptions[control] = (OnErrorsChanged, propertyName);

        control.DetachedFromVisualTree += OnDetached;

        UpdateHasErrorClass(control, vm, propertyName);
    }

    private static void OnDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is not Control control)
            return;
        control.DetachedFromVisualTree -= OnDetached;
        if (_errorSubscriptions.Remove(control, out var sub) && control.DataContext is INotifyDataErrorInfo vm)
            vm.ErrorsChanged -= sub.Handler;
    }

    private static void UpdateHasErrorClass(Control control, INotifyDataErrorInfo vm, string propertyName)
    {
        var errors = vm.GetErrors(propertyName);
        var hasError = errors is IEnumerable en && en.GetEnumerator().MoveNext();
        if (hasError)
            control.Classes.Add("has-error");
        else
            control.Classes.Remove("has-error");
    }
}
