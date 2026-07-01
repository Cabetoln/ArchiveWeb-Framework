using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Archive.API.Core.Contracts;

namespace Archive.Fashion;

/// <summary>
/// Implementação Fashion do ponto flexível <see cref="IImageSearchProvider"/>.
/// Encapsula o serviço de busca por imagem em Python (CLIP + SAM, <c>main.py</c>),
/// encaminhando a imagem ao endpoint <c>POST /search</c> e mapeando os itens
/// retornados para <see cref="ImageSearchResult"/>.
/// </summary>
public class ClipImageSearchProvider(HttpClient http, IConfiguration config) : IImageSearchProvider
{
    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    private readonly string? _baseUrl = config["ImageSearch:BaseUrl"];

    public bool IsAvailable => !string.IsNullOrWhiteSpace(_baseUrl);

    public async Task<IReadOnlyList<ImageSearchResult>> SearchAsync(
        Stream imageStream,
        int topK = 12,
        CancellationToken ct = default)
    {
        if (!IsAvailable)
            return [];

        using var form = new MultipartFormDataContent();
        var imageContent = new StreamContent(imageStream);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(imageContent, "image", "query.jpg");

        using var response = await http.PostAsync($"{_baseUrl!.TrimEnd('/')}/search", form, ct);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        var matches = await JsonSerializer.DeserializeAsync<List<ImageMatchDto>>(stream, JsonOptions, ct) ?? [];

        return matches
            .Where(m => Guid.TryParse(m.Id, out _))
            .Select(m => new ImageSearchResult(Guid.Parse(m.Id!), m.Score))
            .Take(topK)
            .ToList();
    }

    /// <summary>Espelha cada item do JSON retornado por <c>main.py POST /search</c>.</summary>
    private sealed record ImageMatchDto(
        [property: JsonPropertyName("Id")] string? Id,
        [property: JsonPropertyName("score")] float Score);
}
