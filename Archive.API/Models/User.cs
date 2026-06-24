namespace Archive.API.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<WishlistEntry> WishlistEntries { get; set; } = [];

    // Grupos favoritos por chave do schema de domínio (ex: "brand", "author").
    // Generaliza o antigo campo fixo FavoriteBrands — ver IFavoritableGrouping.
    public Dictionary<string, List<string>> FavoriteGroups { get; set; } = [];

    public List<PriceAlert> PriceAlerts { get; set; } = [];
}