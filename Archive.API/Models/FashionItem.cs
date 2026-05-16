namespace Archive.API.Models;

public class FashionItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? ImageUrl { get; set; }
    public string? ProductUrl { get; set; }
    public decimal CurrentPrice { get; set; }
    public string Currency { get; set; } = "BRL";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PriceHistory> PriceHistories { get; set; } = [];
}