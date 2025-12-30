using LifeOS.Application.Abstractions.Images;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Exceptions;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Http;

namespace LifeOS.Application.Features.Images.UploadImage;

public sealed class UploadImageHandler
{
    private readonly IImageStorageService _imageStorageService;
    private readonly LifeOSDbContext _context;

    public UploadImageHandler(
        IImageStorageService imageStorageService,
        LifeOSDbContext context)
    {
        _imageStorageService = imageStorageService;
        _context = context;
    }

    public async Task<ApiResult<UploadImageResponse>> HandleAsync(
        HttpRequest httpRequest,
        CancellationToken cancellationToken)
    {
        if (!httpRequest.HasFormContentType)
        {
            return ApiResultExtensions.Failure<UploadImageResponse>("Content-Type must be multipart/form-data");
        }

        var form = await httpRequest.ReadFormAsync(cancellationToken);
        var file = form.Files.GetFile("file");
        
        if (file == null || file.Length == 0)
        {
            return ApiResultExtensions.Failure<UploadImageResponse>("Geçerli bir dosya seçiniz.");
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

            ImageUploadResult uploadResult = await _imageStorageService.UploadAsync(uploadContext, cancellationToken);

            var normalizedTitle = string.IsNullOrWhiteSpace(title) ? null : title.Trim();

            var image = new Image
            {
                Title = normalizedTitle,
                Size = (int)Math.Min(uploadResult.FileSize, int.MaxValue),
                Path = uploadResult.RelativePath,
                Type = uploadResult.ContentType
            };

            await _context.Images.AddAsync(image, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new UploadImageResponse(
                image.Id,
                uploadResult.StoredFileName,
                uploadResult.ContentType,
                uploadResult.FileSize,
                uploadResult.RelativePath,
                uploadResult.RelativeUrl,
                uploadResult.Width,
                uploadResult.Height);

            return ApiResultExtensions.Success(response, "Resim başarıyla yüklendi");
        }
        catch (ImageStorageException storageException)
        {
            return ApiResultExtensions.Failure<UploadImageResponse>(storageException.Message);
        }
    }
}

