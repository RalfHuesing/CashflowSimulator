namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Ein konkreter Vermögenswert (z. B. ein ETF oder Fonds), den der Nutzer hält.
/// Der Wert entwickelt sich über die Simulation gemäß dem verknüpften ökonomischen Faktor
/// (<see cref="EconomicFactorId"/>). Mehrere Assets können denselben Faktor referenzieren
/// (z. B. drei verschiedene MSCI-World-ETFs), werden aber getrennt mit eigener Stückzahl
/// und Transaktionshistorie geführt.
/// </summary>
public record AssetDto
{
    /// <summary>
    /// Eindeutige ID des Assets (z. B. Guid). Wird für Referenzen und Persistenz genutzt.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Anzeigename des Assets (z. B. "Vanguard FTSE All-World", "iShares MSCI World").
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// ISIN des Wertpapiers (z. B. "IE00B3RBWM25"). Optional für Anzeige und ggf. Kursabfrage.
    /// </summary>
    public string Isin { get; init; } = string.Empty;

    /// <summary>
    /// Art des Vermögenswerts (ETF, Anleihe, Bargeld, Krypto, Immobilie) für Anzeige und ggf. Zuordnung.
    /// </summary>
    public AssetType AssetType { get; init; }

    /// <summary>
    /// ID des ökonomischen Faktors (<see cref="EconomicFactorDto"/>), der die Wertentwicklung
    /// dieses Assets steuert. Die Stochastik (z. B. GBM mit Drift/Volatilität) wird ausschließlich
    /// im Faktor definiert; alle Assets mit derselben EconomicFactorId folgen demselben simulierten
    /// Preis- bzw. Indexpfad (Trennung von Markt und Besitz).
    /// </summary>
    public string EconomicFactorId { get; init; } = string.Empty;

    /// <summary>
    /// Gibt an, ob dieses Asset das aktive Sparplan-Instrument ist. Nur ein Asset (typischerweise
    /// pro Faktor oder pro Nutzerlogik) sollte true sein: Neue Sparraten fließen in dieses Asset.
    /// Weitere Bestände desselben Index (z. B. alter ETF, der nicht mehr bespart wird) haben
    /// false – sie wachsen nur kurstechnisch mit dem Faktor, erhalten aber keine neuen Käufe.
    /// </summary>
    public bool IsActiveSavingsInstrument { get; init; }

    /// <summary>
    /// Steuerliche Einordnung des Fonds/Assets (Aktienfonds, Mischfonds, Anleihenfonds, None)
    /// für die deutsche Besteuerung (Teilfreistellung, Quellensteuer etc.).
    /// </summary>
    public TaxType TaxType { get; init; }

    /// <summary>
    /// Aktuell gehaltene Stückzahl (Anteile). Kann aus der Transaktionshistorie abgeleitet oder
    /// manuell gepflegt werden; für die Simulation ist die konsistente Historie maßgeblich.
    /// </summary>
    public decimal CurrentQuantity { get; init; }

    /// <summary>
    /// Optionaler/berechneter aktueller Wert (Stückzahl × aktueller Kurs). Kann von der Engine
    /// oder UI befüllt werden; muss nicht zwingend persistiert sein, wenn er aus Faktor und
    /// CurrentQuantity berechenbar ist.
    /// Einheit: Währung (decimal).
    /// </summary>
    public decimal? CurrentValue { get; init; }

    /// <summary>
    /// Transaktionshistorie (Käufe, Verkäufe, Ausschüttungen, Vorabpauschale). Notwendig für
    /// eine exakte FIFO-Steuerberechnung bei Verkäufen und für die Nachvollziehbarkeit der
    /// Bestandsentwicklung. Chronologisch sortiert (älteste zuerst) empfohlen.
    /// </summary>
    public List<TransactionDto> Transactions { get; init; } = [];
}
