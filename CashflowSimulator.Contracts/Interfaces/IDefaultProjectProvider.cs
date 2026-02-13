using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Contracts.Interfaces;

/// <summary>
/// Erzeugt ein Standard-Szenario (Default-Projekt) mit allen sinnvollen Voreinstellungen.
/// Implementierung in Engine; Aufrufer (z. B. Desktop) erhalten ein sofort nutzbares SimulationProjectDto.
/// </summary>
public interface IDefaultProjectProvider
{
    /// <summary>
    /// Liefert ein neues Projekt mit Domain-Defaults (Meta, Parameters und künftige Abschnitte).
    /// </summary>
    SimulationProjectDto CreateDefault();
}
