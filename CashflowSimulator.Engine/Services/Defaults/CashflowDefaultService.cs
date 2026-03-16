using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Engine.Services.Defaults;

/// <summary>
/// Lädt die Standard-Cashflows für einen durchschnittlichen deutschen Single-Haushalt 
/// aus einer eingebetteten JSON-Ressource.
/// </summary>
public sealed class CashflowDefaultService : ICashflowDefaultService
{
    private readonly CashflowDefaultsTemplate _template;

    public CashflowDefaultService()
    {
        _template = LoadTemplate() ?? new CashflowDefaultsTemplate();
    }

    private CashflowDefaultsTemplate? LoadTemplate()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CashflowSimulator.Engine.Resources.default_cashflows.json");
        if (stream == null) return null;
        
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        
        return JsonSerializer.Deserialize<CashflowDefaultsTemplate>(stream, options);
    }

    /// <inheritdoc />
    public List<CashflowStreamDto> GetStreams(DateOnly simulationStart, DateOnly simulationEnd, DateOnly dateOfBirth)
    {
        var pensionStart = new DateOnly(dateOfBirth.Year + 67, dateOfBirth.Month, 1);
        var streams = new List<CashflowStreamDto>();

        foreach (var t in _template.Streams)
        {
            streams.Add(new CashflowStreamDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = t.Name,
                Type = t.Type,
                Amount = t.Amount,
                Interval = t.Interval,
                StartDate = ParseDateRef(t.StartDateRef, simulationStart, simulationEnd, pensionStart) ?? simulationStart,
                EndDate = ParseDateRef(t.EndDateRef, simulationStart, simulationEnd, pensionStart),
                StartAge = t.StartAge
            });
        }

        return streams;
    }

    /// <inheritdoc />
    public List<CashflowEventDto> GetEvents(DateOnly simulationStart, DateOnly simulationEnd)
    {
        // Für Events ignorieren wir pensionStart, wir können aber simStart übergeben
        var pensionStart = simulationStart; 
        var events = new List<CashflowEventDto>();

        foreach (var e in _template.Events)
        {
            events.Add(new CashflowEventDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = e.Name,
                Type = e.Type,
                Amount = e.Amount,
                TargetDate = ParseDateRef(e.TargetDateRef, simulationStart, simulationEnd, pensionStart) ?? simulationStart,
                EarliestMonthOffset = e.EarliestMonthOffset,
                LatestMonthOffset = e.LatestMonthOffset
            });
        }

        return events;
    }

    private DateOnly? ParseDateRef(string? reference, DateOnly simStart, DateOnly simEnd, DateOnly pensionStart)
    {
        if (string.IsNullOrEmpty(reference)) return null;
        return reference switch
        {
            "SimulationStart" => simStart,
            "SimulationEnd" => simEnd,
            "PensionStart" => pensionStart,
            "PensionStartMinusOneDay" => pensionStart.AddDays(-1),
            "SimulationStartJuneFirst" => new DateOnly(simStart.Year, 6, 1),
            "SimulationStartDecemberFirst" => new DateOnly(simStart.Year, 12, 1),
            "SimulationStartPlus4Years" => simStart.AddYears(4),
            "SimulationStartPlus5Years" => simStart.AddYears(5),
            "SimulationStartPlus8Years" => simStart.AddYears(8),
            "SimulationStartPlus12Years" => simStart.AddYears(12),
            "SimulationStartPlus20Years" => simStart.AddYears(20),
            "SimulationEndMinus60Months" => simEnd.AddMonths(-60),
            _ => simStart
        };
    }

    // --- JSON Klassen ---
    private class CashflowDefaultsTemplate
    {
        public List<StreamTemplate> Streams { get; set; } = [];
        public List<EventTemplate> Events { get; set; } = [];
    }

    private class StreamTemplate
    {
        public string Name { get; set; } = "";
        public CashflowType Type { get; set; }
        public decimal Amount { get; set; }
        public CashflowInterval Interval { get; set; }
        public string? StartDateRef { get; set; }
        public string? EndDateRef { get; set; }
        public int? StartAge { get; set; }
    }

    private class EventTemplate
    {
        public string Name { get; set; } = "";
        public CashflowType Type { get; set; }
        public decimal Amount { get; set; }
        public string? TargetDateRef { get; set; }
        public int EarliestMonthOffset { get; set; }
        public int LatestMonthOffset { get; set; }
    }
}