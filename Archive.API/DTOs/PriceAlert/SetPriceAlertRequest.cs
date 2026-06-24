namespace Archive.API.DTOs;

public record SetPriceAlertRequest(Guid ProductId, decimal TargetPrice);
