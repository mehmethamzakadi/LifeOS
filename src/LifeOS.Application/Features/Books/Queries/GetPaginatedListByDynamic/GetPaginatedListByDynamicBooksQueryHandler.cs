using AutoMapper;
using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Common.Responses;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Repositories;
using MediatR;

namespace LifeOS.Application.Features.Books.Queries.GetPaginatedListByDynamic;

public sealed class GetPaginatedListByDynamicBooksQueryHandler(
    IBookRepository bookRepository,
    IMapper mapper,
    ICacheService cacheService) : IRequestHandler<GetPaginatedListByDynamicBooksQuery, PaginatedListResponse<GetPaginatedListByDynamicBooksResponse>>
{
    public async Task<PaginatedListResponse<GetPaginatedListByDynamicBooksResponse>> Handle(GetPaginatedListByDynamicBooksQuery request, CancellationToken cancellationToken)
    {
        var pagination = request.DataGridRequest.PaginatedRequest;
        var versionKey = CacheKeys.BookGridVersion();
        var versionToken = await cacheService.Get<string>(versionKey);
        if (string.IsNullOrWhiteSpace(versionToken))
        {
            versionToken = Guid.NewGuid().ToString("N");
            await cacheService.Add(versionKey, versionToken, null, null);
        }

        var cacheKey = CacheKeys.BookGrid(versionToken, pagination.PageIndex, pagination.PageSize, request.DataGridRequest.DynamicQuery);
        var cachedResponse = await cacheService.Get<PaginatedListResponse<GetPaginatedListByDynamicBooksResponse>>(cacheKey);
        if (cachedResponse is not null)
        {
            return cachedResponse;
        }

        Paginate<Book> booksDynamic = await bookRepository.GetPaginatedListByDynamicAsync(
            dynamic: request.DataGridRequest.DynamicQuery,
            index: pagination.PageIndex,
            size: pagination.PageSize,
            include: null,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        PaginatedListResponse<GetPaginatedListByDynamicBooksResponse> response = mapper.Map<PaginatedListResponse<GetPaginatedListByDynamicBooksResponse>>(booksDynamic);
        await cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.BookGrid),
            null);

        return response;
    }
}

