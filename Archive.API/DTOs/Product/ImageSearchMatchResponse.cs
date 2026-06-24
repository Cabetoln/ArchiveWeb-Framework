namespace Archive.API.DTOs;

/// <summary>Produto retornado pela busca por imagem, com seu score de similaridade.</summary>
public record ImageSearchMatchResponse(
    ProductResponse Product,
    float Score
);
