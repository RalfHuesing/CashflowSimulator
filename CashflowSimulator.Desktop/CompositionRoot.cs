using System;

namespace CashflowSimulator.Desktop;

/// <summary>
/// Composition Root – Zugriff auf den zentralen ServiceProvider.
/// Wird in Program.Main gesetzt, bevor die App startet.
/// </summary>
internal static class CompositionRoot
{
    public static IServiceProvider Services { get; set; } = null!;
}
