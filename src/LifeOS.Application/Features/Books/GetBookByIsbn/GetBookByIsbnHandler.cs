using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Services;

namespace LifeOS.Application.Features.Books.GetBookByIsbn;

public sealed class GetBookByIsbnHandler
{
    private readonly IBookService _bookService;

    public GetBookByIsbnHandler(IBookService bookService)
    {
        _bookService = bookService;
    }

    public async Task<ApiResult<GetBookByIsbnResponse>> HandleAsync(
        GetBookByIsbnQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var bookInfo = await _bookService.GetBookByIsbnAsync(query.Isbn, cancellationToken);
            var response = new GetBookByIsbnResponse(bookInfo);
            return ApiResultExtensions.Success(response, ResponseMessages.Book.Retrieved);
        }
        catch (ArgumentException ex)
        {
            return ApiResultExtensions.Failure<GetBookByIsbnResponse>(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            return ApiResultExtensions.Failure<GetBookByIsbnResponse>(
                $"Kitap bilgisi alınamadı: {ex.Message}");
        }
        catch (TimeoutException ex)
        {
            return ApiResultExtensions.Failure<GetBookByIsbnResponse>(
                $"İstek zaman aşımına uğradı: {ex.Message}");
        }
    }
}

