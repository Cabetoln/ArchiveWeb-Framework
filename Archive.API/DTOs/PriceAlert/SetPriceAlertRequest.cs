namespace Archive.API.DTOs;

public record SetPriceAlertRequest(Guid FashionItemId, decimal TargetPrice);
