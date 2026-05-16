namespace Archive.API.DTOs;

public record AddToWishlistRequest(Guid FashionItemId, string? Note = null);
