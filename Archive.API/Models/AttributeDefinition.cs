namespace Archive.API.Models;

public record AttributeDefinition(
    string Key,
    string DisplayName,
    AttributeType Type,
    bool IsFilterable = false,
    bool IsRequired = false,
    string[]? AllowedValues = null
);

public enum AttributeType { Text, Number, Enum }
