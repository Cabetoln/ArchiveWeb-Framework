namespace Archive.API.Models;

public class PriceHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FashionItemId { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "BRL";
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public string? Source { get; set; }

    public FashionItem FashionItem { get; set; } = null!;
}