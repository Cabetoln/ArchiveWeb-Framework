using System.Diagnostics;
using System.Text.Json;
using Archive.API.Core.Contracts;

namespace Archive.Fashion;

/// <summary>
/// Implementação Fashion do ponto flexível <see cref="IPriceScraper"/>.
/// Encapsula o scraper da Farfetch escrito em Python (<c>scrape_prices.py</c>),
/// executando-o em modo <c>--scrape-only</c> e convertendo a saída JSON em
/// <see cref="ScrapedProduct"/>. Os campos de domínio (marca/categoria) são
/// devolvidos no dicionário <see cref="ScrapedProduct.Attributes"/>.
/// </summary>
public class FarfetchPriceScraper(IConfiguration config, IHostEnvironment env) : IPriceScraper
{
    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    public string Name => "farfetch_scraper";

    public async Task<IReadOnlyList<ScrapedProduct>> ScrapeAsync(CancellationToken ct = default)
    {
        var python = config["FashionScraper:PythonPath"] ?? "python3";
        var scriptRelative = config["FashionScraper:ScriptPath"] ?? "Fashion/scrape_prices.py";
        var scriptPath = Path.GetFullPath(Path.Combine(env.ContentRootPath, scriptRelative));

        if (!File.Exists(scriptPath))
            throw new FileNotFoundException($"Script do scraper não encontrado: {scriptPath}");

        var psi = new ProcessStartInfo
        {
            FileName = python,
            WorkingDirectory = Path.GetDirectoryName(scriptPath)!,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        psi.ArgumentList.Add(scriptPath);
        psi.ArgumentList.Add("--scrape-only");

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Falha ao iniciar o processo do scraper.");

        var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
        var stderrTask = process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        var stdout = await stdoutTask;
        if (process.ExitCode != 0)
        {
            var stderr = await stderrTask;
            throw new InvalidOperationException(
                $"Scraper Python falhou (exit {process.ExitCode}): {stderr}");
        }

        var scraped = JsonSerializer.Deserialize<List<ScrapedItemDto>>(stdout, JsonOptions) ?? [];

        return scraped.Select(item => new ScrapedProduct(
            Name: item.Name,
            ProductUrl: item.ProductUrl,
            ImageUrl: item.ImageUrl,
            Price: (decimal)item.Price,
            Currency: string.IsNullOrWhiteSpace(item.Currency) ? "BRL" : item.Currency,
            Attributes: new Dictionary<string, string?>
            {
                ["brand"] = item.Brand,
                ["category"] = item.Category,
            }
        )).ToList();
    }

    /// <summary>Espelha o JSON emitido por <c>scrape_prices.py --scrape-only</c>.</summary>
    private sealed record ScrapedItemDto(
        string Name,
        string? Brand,
        string? Category,
        string? ImageUrl,
        string? ProductUrl,
        double Price,
        string? Currency);
}
