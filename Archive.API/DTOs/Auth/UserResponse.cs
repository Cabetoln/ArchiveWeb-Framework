namespace Archive.API.DTOs;

public record UserResponse(Guid Id, string Name, string Email, DateTime CreatedAt);
