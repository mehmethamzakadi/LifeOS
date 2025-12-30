using LifeOS.Application.Common.Responses;

namespace LifeOS.API.Common;

/// <summary>
/// API response model for standardized responses
/// Re-exports Application.Common.Responses.ApiResult for backward compatibility
/// </summary>
public class ApiResult<T> : LifeOS.Application.Common.Responses.ApiResult<T>
{
}

/// <summary>
/// API response model without generic type parameter
/// Re-exports Application.Common.Responses.ApiReturn for backward compatibility
/// </summary>
public class ApiReturn : LifeOS.Application.Common.Responses.ApiReturn
{
}

