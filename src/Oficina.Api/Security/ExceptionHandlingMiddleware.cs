using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oficina.Application;
using Oficina.Application.Abstractions.Persistence;

namespace Oficina.Api.Security;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            var (status, title, detail) = exception switch
            {
                RecursoNaoEncontradoException => (StatusCodes.Status404NotFound, "Recurso não encontrado", exception.Message),
                ArgumentException => (StatusCodes.Status400BadRequest, "Requisição inválida", exception.Message),
                InvalidOperationException => (StatusCodes.Status409Conflict, "Operação não permitida", exception.Message),
                ConcurrencyConflictException => (StatusCodes.Status409Conflict, "Conflito de concorrência", exception.Message),
                DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Conflito de concorrência", "Os dados mudaram durante a operação. Recarregue e tente novamente."),
                DbUpdateException => (StatusCodes.Status409Conflict, "Conflito de persistência", "A operação conflita com dados já persistidos."),
                _ => (StatusCodes.Status500InternalServerError, "Erro interno", "Ocorreu um erro inesperado.")
            };

            if (status == StatusCodes.Status500InternalServerError) logger.LogError(exception, "Erro não tratado na API.");
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            });
        }
    }
}
