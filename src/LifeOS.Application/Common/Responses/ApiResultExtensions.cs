using Microsoft.AspNetCore.Http;

namespace LifeOS.Application.Common.Responses;

/// <summary>
/// Extension methods for creating standardized ApiResult responses.
/// Provides consistent response formatting across all endpoints.
/// </summary>
public static class ApiResultExtensions
{
    /// <summary>
    /// Creates a successful ApiResult response with data.
    /// </summary>
    public static ApiResult<T> Success<T>(T data, string message = "İşlem başarılı")
    {
        return new ApiResult<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Creates a successful ApiResult response without data.
    /// </summary>
    public static ApiResult<object> Success(string message = "İşlem başarılı")
    {
        return new ApiResult<object>
        {
            Success = true,
            Message = message
        };
    }

    /// <summary>
    /// Creates a failed ApiResult response with error message.
    /// </summary>
    public static ApiResult<T> Failure<T>(string message, List<string>? errors = null)
    {
        return new ApiResult<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    /// <summary>
    /// Creates a failed ApiResult response without data.
    /// </summary>
    public static ApiResult<object> Failure(string message, List<string>? errors = null)
    {
        return new ApiResult<object>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    /// <summary>
    /// Converts ApiResult to IResult for minimal API endpoints.
    /// </summary>
    public static IResult ToResult<T>(this ApiResult<T> apiResult)
    {
        if (!apiResult.Success)
        {
            // Determine status code based on error type
            if (apiResult.Errors.Any(e => e.Contains("bulunamadı") || e.Contains("NotFound")))
            {
                return Results.NotFound(apiResult);
            }

            if (apiResult.Errors.Any(e => e.Contains("yetki") || e.Contains("Unauthorized")))
            {
                return Results.Unauthorized();
            }

            return Results.BadRequest(apiResult);
        }

        return Results.Ok(apiResult);
    }

    /// <summary>
    /// Converts ApiResult to IResult with custom success status code.
    /// </summary>
    public static IResult ToResult<T>(this ApiResult<T> apiResult, int successStatusCode)
    {
        if (!apiResult.Success)
        {
            return apiResult.ToResult();
        }

        return successStatusCode switch
        {
            StatusCodes.Status201Created => Results.Created(string.Empty, apiResult),
            StatusCodes.Status204NoContent => Results.NoContent(),
            _ => Results.Ok(apiResult)
        };
    }

    /// <summary>
    /// Creates a successful response and converts to IResult with 201 Created status.
    /// </summary>
    public static IResult CreatedResult<T>(T data, string location, string message = "Kayıt başarıyla oluşturuldu")
    {
        var apiResult = Success(data, message);
        return Results.Created(location, apiResult);
    }

    /// <summary>
    /// Creates a successful response and converts to IResult with 204 NoContent status.
    /// </summary>
    public static IResult NoContentResult(string message = "İşlem başarılı")
    {
        var apiResult = Success(message);
        return Results.Ok(apiResult); // 204 NoContent doesn't support body, so we use 200 with message
    }

    /// <summary>
    /// Creates a validation error response from FluentValidation errors.
    /// </summary>
    public static ApiResult<object> ValidationError(List<string> errors)
    {
        return Failure("Doğrulama hatası", errors);
    }
}

