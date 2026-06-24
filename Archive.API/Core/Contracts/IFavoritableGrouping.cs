using Archive.API.Models;

namespace Archive.API.Core.Contracts;

public interface IFavoritableGrouping
{
    string Key { get; }
    string DisplayName { get; }

    string? ExtractValue(Product product)
        => product.Attributes.GetValueOrDefault(Key);
}
