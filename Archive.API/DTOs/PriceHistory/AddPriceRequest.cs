namespace Archive.API.DTOs;

public record AddPriceRequest(decimal Price, string Currency = "BRL", string? Source = null);
