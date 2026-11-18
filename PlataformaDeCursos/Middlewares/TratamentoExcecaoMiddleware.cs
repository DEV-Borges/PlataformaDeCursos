using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using PlataformaDeCursos.Exceptions;

namespace PlataformaDeCursos.Middlewares;

public class TratamentoExcecaoMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TratamentoExcecaoMiddleware> _logger;

    public TratamentoExcecaoMiddleware(
        RequestDelegate next,
        ILogger<TratamentoExcecaoMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        } catch (Exception ex)
        {
            await TratarExcecaoAsync(context, ex);
        }
    }

    private async Task TratarExcecaoAsync(HttpContext context, Exception ex)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        var (status, titulo, detalhe) = ex switch
        {
            RecursoNaoEncontradoException => (
                StatusCodes.Status404NotFound,
                "Recurso não encontrado",
                ex.Message),

            ConflitoDeDadosException => (
                StatusCodes.Status409Conflict,
                "Conflito de dados",
                ex.Message),

            RegraDeNegocioException => (
                StatusCodes.Status422UnprocessableEntity,
                "Violação de regra de negócio",
                ex.Message),

            // Captura violação de índice único diretamente do MySQL (erro 1062 = Duplicate entry)
            DbUpdateException dbEx when dbEx.InnerException is MySqlException { Number: 1062 } => (
                StatusCodes.Status409Conflict,
                "Registro duplicado",
                "Operação negada pois viola uma restrição de unicidade (e-mail ou matrícula já existente)."),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno no servidor",
                "Ocorreu uma falha inesperada ao processar a solicitação.")
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(ex, "Falha crítica na requisição {Method} {Path} | TraceId: {TraceId}",
                context.Request.Method, context.Request.Path, traceId);
        } else
        {
            _logger.LogWarning("Exceção de negócio ({Status}) em {Method} {Path}: {Detalhe} | TraceId: {TraceId}",
                status, context.Request.Method, context.Request.Path, detalhe, traceId);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = titulo,
            Detail = detalhe,
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = traceId;

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem);
    }
}