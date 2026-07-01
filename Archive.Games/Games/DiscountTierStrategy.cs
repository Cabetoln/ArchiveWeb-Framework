namespace Archive.Games;

/// <summary>
/// <b>Padrão Strategy.</b> Encapsula a política de faixas de desconto do domínio
/// de games, isolando-a de quem a usa (o schema e o scraper). Trocar a política
/// (ex.: outras faixas, outra moeda de comparação) é registrar outra
/// implementação na raiz de composição, sem tocar no resto do plugin.
/// </summary>
public interface IDiscountTierStrategy
{
    /// <summary>Faixas possíveis, na ordem em que devem aparecer na UI/filtros.</summary>
    IReadOnlyList<string> Tiers { get; }

    /// <summary>Classifica um percentual de desconto (0–100) em uma das <see cref="Tiers"/>.</summary>
    string Classify(decimal savingsPercent);
}

/// <summary>
/// Implementação padrão: agrupa o desconto em faixas percentuais fixas.
/// </summary>
public sealed class PercentageDiscountTierStrategy : IDiscountTierStrategy
{
    private const string Free = "Grátis";
    private const string Huge = "75%+";
    private const string Big = "50–75%";
    private const string Mid = "25–50%";
    private const string Small = "Até 25%";

    public IReadOnlyList<string> Tiers { get; } = [Free, Huge, Big, Mid, Small];

    public string Classify(decimal savingsPercent) => savingsPercent switch
    {
        >= 100 => Free,
        >= 75 => Huge,
        >= 50 => Big,
        >= 25 => Mid,
        _ => Small,
    };
}
