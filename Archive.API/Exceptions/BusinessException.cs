namespace Archive.API.Exceptions;

public sealed class BusinessException : Exception
{
    public int StatusCode { get; }

    public BusinessException(string message, int statusCode = StatusCodes.Status400BadRequest)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
