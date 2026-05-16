using Microsoft.AspNetCore.Diagnostics;

namespace Archive.API.Exceptions;

/// <summary>
/// Middleware de tratamento global de exceções.
/// Captura qualquer exceção não tratada e retorna uma resposta JSON padronizada.
/// BusinessException gera respostas de erro de negócio (4xx).
/// Demais exceções geram 500 com mensagem genérica, sem expor detalhes internos.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        context.Response.ContentType = "application/json";

        if (exception is BusinessException businessException)
        {
            context.Response.StatusCode = businessException.StatusCode;
            await context.Response.WriteAsJsonAsync(
                new { error = businessException.Message },
                cancellationToken);
            return true;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(
            new { error = "Ocorreu um erro interno no servidor." },
            cancellationToken);
        return true;
    }
}
