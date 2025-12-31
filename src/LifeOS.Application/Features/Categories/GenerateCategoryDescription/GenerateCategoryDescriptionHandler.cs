using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Services;

namespace LifeOS.Application.Features.Categories.GenerateCategoryDescription;

public sealed class GenerateCategoryDescriptionHandler
{
    private readonly IAiService _aiService;

    public GenerateCategoryDescriptionHandler(IAiService aiService)
    {
        _aiService = aiService;
    }

    public async Task<ApiResult<GenerateCategoryDescriptionResponse>> HandleAsync(
        string categoryName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return ApiResultExtensions.Failure<GenerateCategoryDescriptionResponse>(
                "Kategori adı boş olamaz.");
        }

        try
        {
            var description = await _aiService.GenerateCategoryDescriptionAsync(
                categoryName,
                cancellationToken);

            var response = new GenerateCategoryDescriptionResponse(description);

            return ApiResultExtensions.Success(
                response,
                "Kategori açıklaması başarıyla üretildi");
        }
        catch (Exception ex)
        {
            return ApiResultExtensions.Failure<GenerateCategoryDescriptionResponse>(
                $"Açıklama üretilirken hata oluştu: {ex.Message}");
        }
    }
}

