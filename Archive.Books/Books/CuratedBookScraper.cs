using Archive.API.Core.Contracts;

namespace Archive.Books;

/// <summary>
/// Realização do ponto flexível <see cref="IPriceScraper"/> para o domínio de livros.
/// Diferente do plugin Fashion (que orquestra um processo Python), esta fonte é
/// escrita inteiramente em C# — um feed curado em memória. Demonstra que o
/// acoplamento a Python era uma escolha da implementação Fashion, não do framework:
/// o núcleo só enxerga <see cref="IPriceScraper"/>.
/// </summary>
public class CuratedBookScraper : IPriceScraper
{
    public string Name => "curated_book_feed";

    public Task<IReadOnlyList<ScrapedProduct>> ScrapeAsync(CancellationToken ct = default)
    {
        IReadOnlyList<ScrapedProduct> feed =
        [
            Book("Duna", "Frank Herbert", "Ficção", "Brochura", 79.90m,
                "https://livraria.example.com/duna"),
            Book("O Senhor dos Anéis", "J.R.R. Tolkien", "Fantasia", "Capa dura", 149.90m,
                "https://livraria.example.com/senhor-dos-aneis"),
            Book("Código Limpo", "Robert C. Martin", "Técnico", "Brochura", 119.00m,
                "https://livraria.example.com/codigo-limpo"),
            Book("A Menina que Roubava Livros", "Markus Zusak", "Ficção", "E-book", 34.90m,
                "https://livraria.example.com/menina-que-roubava-livros"),
        ];

        return Task.FromResult(feed);
    }

    private static ScrapedProduct Book(
        string title, string author, string genre, string format, decimal price, string url) =>
        new(
            Name: title,
            ProductUrl: url,
            ImageUrl: null,
            Price: price,
            Currency: "BRL",
            Attributes: new Dictionary<string, string?>
            {
                ["author"] = author,
                ["genre"]  = genre,
                ["format"] = format,
            });
}
