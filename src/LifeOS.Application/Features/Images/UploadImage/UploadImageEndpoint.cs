using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Images.UploadImage;

public static class UploadImageEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/images", async (
            HttpRequest httpRequest,
            UploadImageHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(httpRequest, cancellationToken);
            return result.ToResult();
        })
        .WithName("UploadImage")
        .WithTags("Images")
        .RequireAuthorization(Domain.Constants.Permissions.MediaUpload)
        .Accepts<IFormFile>("multipart/form-data")
        .Produces<ApiResult<UploadImageResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<UploadImageResponse>>(StatusCodes.Status400BadRequest);
    }
}

