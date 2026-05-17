namespace Archive.API.Models;

public class PriceAlert
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid FashionItemId { get; set; }
    public decimal TargetPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
