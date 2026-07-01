using System.Text.Json;
using System.Text.Json.Serialization;
using Archive.API.Core.Contracts;

namespace Archive.Books;

/// <summary>
/// Realização do ponto flexível <see cref="IPriceScraper"/> para o domínio de
/// livros. Faz uma busca HTTP real na API pública do Open Library
/// (<c>openlibrary.org/search.json</c>), em C# puro — sem Python.
///
/// Observação honesta: o Open Library fornece metadados reais (título, autor,
/// capa), mas NÃO fornece preço (nenhuma API gratuita de livros fornece). Como o
/// app é de monitoramento de preço, o preço é <b>derivado de forma determinística</b>
/// a partir do número de páginas / chave da obra — estável entre sincronizações,
/// para que o dedup por URL funcione. Para preços reais, bastaria trocar a fonte
/// por uma com <c>saleInfo</c> (ex.: Google Books com API key).
/// </summary>
public class OpenLibraryPriceScraper(HttpClient http, IConfiguration config) : IPriceScraper
{
    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    public string Name => "openlibrary_feed";

    public async Task<IReadOnlyList<ScrapedProduct>> ScrapeAsync(CancellationToken ct = default)
    {
        var baseUrl = config["OpenLibrary:BaseUrl"] ?? "https://openlibrary.org/search.json";
        var query   = config["OpenLibrary:Query"]   ?? "subject:fiction";
        var genre   = config["OpenLibrary:Genre"]   ?? "Ficção";
        var limit   = int.TryParse(config["OpenLibrary:Limit"], out var l) ? l : 15;

        var url = $"{baseUrl}?q={Uri.EscapeDataString(query)}&limit={limit}" +
                  "&fields=title,author_name,cover_i,key,number_of_pages_median";

        using var response = await http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        var payload = await JsonSerializer.DeserializeAsync<SearchResponse>(stream, JsonOptions, ct);

        var docs = payload?.Docs ?? [];

        return docs
            .Where(d => !string.IsNullOrWhiteSpace(d.Title) && !string.IsNullOrWhiteSpace(d.Key))
            .Select(d => new ScrapedProduct(
                Name:       d.Title!.Trim(),
                ProductUrl: $"https://openlibrary.org{d.Key}",
                ImageUrl:   d.Cover_i is int c ? $"https://covers.openlibrary.org/b/id/{c}-M.jpg" : null,
                Price:      DerivePrice(d),
                Currency:   "BRL",
                Attributes: new Dictionary<string, string?>
                {
                    ["author"] = d.Author_name?.FirstOrDefault(),
                    ["genre"]  = genre,
                }))
            .ToList();
    }

    /// <summary>
    /// Preço sintético e <b>determinístico</b> (o Open Library não expõe preço).
    /// Baseado no nº de páginas quando disponível, senão no hash estável da chave
    /// da obra — assim o mesmo livro sempre gera o mesmo preço e o sync não fica
    /// marcando "atualizado" à toa.
    /// </summary>
    private static decimal DerivePrice(Doc d)
    {
        var seed = d.Number_of_pages_median
                   ?? (Math.Abs(StableHash(d.Key!)) % 400 + 80);
        var value = 19m + (seed % 130); // faixa ~R$19–R$149
        return Math.Round(value, 0) - 0.10m; // termina em ,90
    }

    private static int StableHash(string s)
    {
        unchecked
        {
            int hash = 23;
            foreach (var ch in s) hash = hash * 31 + ch;
            return hash;
        }
    }

    /// <summary>Espelha os campos pedidos em <c>search.json?fields=...</c>.</summary>
    private sealed record SearchResponse([property: JsonPropertyName("docs")] List<Doc>? Docs);

    private sealed record Doc(
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("author_name")] string[]? Author_name,
        [property: JsonPropertyName("cover_i")] int? Cover_i,
        [property: JsonPropertyName("key")] string? Key,
        [property: JsonPropertyName("number_of_pages_median")] int? Number_of_pages_median);
}
