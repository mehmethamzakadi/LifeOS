using LifeOS.Application.Abstractions.Images;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Exceptions;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Images.Endpoints;

public static class UploadImage
{
    public sealed record Response(
        Guid ImageId,
        string FileName,
        string ContentType,
        long Size,
        string RelativePath,
        string Url,
        int Width,
        int Height);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/images", async (
            HttpRequest httpRequest,
            IImageStorageService imageStorageService,
            LifeOSDbContext context,
            CancellationToken cancellationToken) =>
        {
            if (!httpRequest.HasFormContentType)
            {
                return Results.BadRequest(new { Error = "Content-Type must be multipart/form-data" });
            }

            var form = await httpRequest.ReadFormAsync(cancellationToken);
            var file = form.Files.GetFile("file");
            
            if (file == null || file.Length == 0)
            {
                return Results.BadRequest(new { Error = "Geçerli bir dosya seçiniz." });
            }

            var scope = form["scope"].ToString();
            var maxWidthStr = form["maxWidth"].ToString();
            var maxHeightStr = form["maxHeight"].ToString();
            var resizeModeStr = form["resizeMode"].ToString();
            var title = form["title"].ToString();

            int? maxWidth = int.TryParse(maxWidthStr, out var w) ? w : null;
            int? maxHeight = int.TryParse(maxHeightStr, out var h) ? h : null;
            var resizeMode = Enum.TryParse<ImageResizeMode>(resizeModeStr, out var mode) ? mode : ImageResizeMode.Fit;

            try
            {
                await using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream, cancellationToken);

                var uploadContext = new ImageUploadContext
                {
                    Content = memoryStream,
                    FileName = file.FileName,
                    ContentType = file.ContentType ?? string.Empty,
                    FileSize = file.Length,
                    Scope = string.IsNullOrWhiteSpace(scope) ? string.Empty : scope.Trim(),
                    Resize = maxWidth is null && maxHeight is null
                        ? null
                        : new ImageResizeOptions
                        {
                            Width = maxWidth,
                            Height = maxHeight,
                            Mode = resizeMode
                        }
                };

                ImageUploadResult uploadResult = await imageStorageService.UploadAsync(uploadContext, cancellationToken);

                var normalizedTitle = string.IsNullOrWhiteSpace(title) ? null : title.Trim();

                var image = new Image
                {
                    Title = normalizedTitle,
                    Size = (int)Math.Min(uploadResult.FileSize, int.MaxValue),
                    Path = uploadResult.RelativePath,
                    Type = uploadResult.ContentType
                };

                await context.Images.AddAsync(image, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                var response = new Response(
                    image.Id,
                    uploadResult.StoredFileName,
                    uploadResult.ContentType,
                    uploadResult.FileSize,
                    uploadResult.RelativePath,
                    uploadResult.RelativeUrl,
                    uploadResult.Width,
                    uploadResult.Height);

                return Results.Ok(response);
            }
            catch (ImageStorageException storageException)
            {
                return Results.BadRequest(new { Error = storageException.Message });
            }
        })
        .WithName("UploadImage")
        .WithTags("Images")
        .RequireAuthorization(Domain.Constants.Permissions.MediaUpload)
        .Accepts<IFormFile>("multipart/form-data")
        .Produces<Response>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

