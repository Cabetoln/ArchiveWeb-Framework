namespace Archive.API.Core.Contracts;

public interface IImageSearchProvider
{
    bool IsAvailable { get; }

    Task<IReadOnlyList<ImageSearchResult>> SearchAsync(
        Stream imageStream,
        int topK = 12,
        CancellationToken ct = default);
}

public record ImageSearchResult(Guid ProductId, float Score);
