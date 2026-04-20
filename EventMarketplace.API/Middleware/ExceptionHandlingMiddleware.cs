using EventMarketplace.Application.Exceptions;
using EventMarketplace.API.Responses;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace EventMarketplace.API.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(ex, "Validation error on {Path}", httpContext.Request.Path);
            await WriteValidationProblem(httpContext, ex);
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning(ex, "Resource not found: {Message}", ex.Message);
            await WriteProblem(httpContext, ex.Message, (int)HttpStatusCode.NotFound, "Not Found");
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized access on {Path}", httpContext.Request.Path);
            await WriteProblem(httpContext, "Unauthorized.", (int)HttpStatusCode.Unauthorized, "Unauthorized");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception on {Path}", httpContext.Request.Path);
            await WriteProblem(httpContext, "An unexpected error occurred.", (int)HttpStatusCode.InternalServerError, "Internal Server Error");
        }
    }

    private static async Task WriteValidationProblem(HttpContext httpContext, ValidationException ex)
    {
        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        httpContext.Response.ContentType = "application/json";

        var validationErrors = ex.Errors
            .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
            .ToList();

        var response = CustomResponse<NoContent>.Fail(
            new ErrorDto(validationErrors, true),
            (int)HttpStatusCode.BadRequest);

        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private static async Task WriteProblem(HttpContext httpContext, string detail, int status, string title)
    {
        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/json";

        var response = CustomResponse<NoContent>.Fail(
            new ErrorDto($"{title}: {detail}", true),
            status);

        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
