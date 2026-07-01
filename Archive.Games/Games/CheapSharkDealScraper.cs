using Archive.API.Core.Contracts;

namespace Archive.Games;

/// <summary>
/// Realização do ponto flexível <see cref="IPriceScraper"/> para o domínio de
/// games. Faz uma busca HTTP real na API pública da CheapShark, em C# puro — sem
/// Python. Ao contrário de fontes de livros, a CheapShark expõe <b>preço e
/// desconto reais</b> agregados de várias lojas (Steam, GOG, Humble Bundle...),
/// então o monitoramento de preço, os alertas e a análise sazonal do núcleo
/// operam sobre dados verdadeiros.
///
/// A identidade do produto é o <c>dealID</c> da CheapShark, que é <b>persistente</b>
/// por combinação jogo×loja (o preço muda, o <c>dealID</c> permanece). Isso faz o
/// dedup por <c>ProductUrl</c> do núcleo acumular corretamente o histórico de
/// preços a cada sincronização.
///
/// Colabora com dois pontos de flexibilidade internos do plugin:
/// a <see cref="CheapSharkClient"/> (Facade que esconde a API externa) e a
/// <see cref="IDiscountTierStrategy"/> (Strategy que classifica a faixa de desconto).
/// </summary>
public class CheapSharkDealScraper(CheapSharkClient client, IDiscountTierStrategy discountTiers)
    : IPriceScraper
{
    public string Name => "cheapshark_deals";

    public async Task<IReadOnlyList<ScrapedProduct>> ScrapeAsync(CancellationToken ct = default)
    {
        var deals = await client.GetDealsAsync(ct);

        return deals.Select(d => new ScrapedProduct(
            Name:       d.Title,
            ProductUrl: $"https://www.cheapshark.com/redirect?dealID={d.DealId}",
            ImageUrl:   d.Thumb,
            Price:      d.SalePrice,
            Currency:   "USD",
            Attributes: new Dictionary<string, string?>
            {
                ["store"]       = d.StoreName,
                ["rating"]      = d.RatingText ?? "Sem avaliação",
                ["discount"]    = discountTiers.Classify(d.SavingsPercent),
                // atributos apenas de exibição (não filtráveis) — permitem à UI
                // mostrar "de $X por $Y (-Z%)" no estilo de um site de ofertas.
                ["normalPrice"] = d.NormalPrice.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                ["savings"]     = Math.Round(d.SavingsPercent).ToString(System.Globalization.CultureInfo.InvariantCulture),
            }
        )).ToList();
    }
}
