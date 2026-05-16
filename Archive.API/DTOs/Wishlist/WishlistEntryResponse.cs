namespace Archive.API.DTOs;

public record WishlistEntryResponse(
    Guid Id,
    Guid FashionItemId,
    string ItemName,
    string Brand,
    decimal CurrentPrice,
    string? ImageUrl,
    DateTime AddedAt,
    string? Note
);
