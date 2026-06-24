namespace Archive.API.DTOs;

public record AddToWishlistRequest(Guid ProductId, string? Note = null);
