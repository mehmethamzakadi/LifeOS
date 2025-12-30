namespace LifeOS.Application.Features.Images.UploadImage;

public sealed record UploadImageResponse(
    Guid ImageId,
    string FileName,
    string ContentType,
    long Size,
    string RelativePath,
    string Url,
    int Width,
    int Height);

