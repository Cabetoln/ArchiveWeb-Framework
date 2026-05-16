namespace Archive.API.Models;

public class WishlistEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid FashionItemId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }

    public User User { get; set; } = null!;
    public FashionItem FashionItem { get; set; } = null!;
}