using System.Net;
using System.Text.Json;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Exceptions;

namespace ECommerce.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message) = ex switch
        {
            NotFoundException => (HttpStatusCode.NotFound, ex.Message),
            BadRequestException => (HttpStatusCode.BadRequest, ex.Message),
            UnauthorizedException => (HttpStatusCode.Unauthorized, ex.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred"),
        };

        var response = new ErrorResponse
        {
            StatusCode = (int)statusCode,
            Message = message,
            Details =
                ex is not NotFoundException
                && ex is not BadRequestException
                && ex is not UnauthorizedException
                    ? ex.Message
                    : null,
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );

        await context.Response.WriteAsync(json);
    }
}
