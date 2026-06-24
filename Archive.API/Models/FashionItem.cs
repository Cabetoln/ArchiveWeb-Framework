namespace Archive.API.Models;

public class Product
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public string Currency { get; set; } = "BRL";
    public string? ImageUrl { get; set; }
    public string? ProductUrl { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // campos extras definidos pelo domínio (brand, category, author, platform...)
    public Dictionary<string, string?> Attributes { get; init; } = [];
}