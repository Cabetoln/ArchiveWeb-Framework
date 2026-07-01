using Archive.API.Core.Contracts;

namespace Archive.Games;

/// <summary>
/// Realização do ponto flexível <see cref="IImageSearchProvider"/> para o domínio
/// de games, que não oferece busca por imagem. Segue o padrão null-object:
/// <see cref="IsAvailable"/> é <c>false</c> e a busca devolve lista vazia, então
/// o núcleo continua funcionando sem exigir um serviço de visão computacional.
/// </summary>
public class NoOpImageSearchProvider : IImageSearchProvider
{
    public bool IsAvailable => false;

    public Task<IReadOnlyList<ImageSearchResult>> SearchAsync(
        Stream imageStream,
        int topK = 12,
        CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<ImageSearchResult>>([]);
}
