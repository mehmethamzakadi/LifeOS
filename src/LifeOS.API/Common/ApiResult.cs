namespace LifeOS.API.Common;

/// <summary>
/// API response model for standardized responses
/// </summary>
public class ApiResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string InternalMessage { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// API response model without generic type parameter
/// </summary>
public class ApiReturn : ApiResult<object>
{
}

