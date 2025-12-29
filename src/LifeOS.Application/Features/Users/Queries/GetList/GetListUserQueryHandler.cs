using AutoMapper;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Common.Responses;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Repositories;
using MediatR;

namespace LifeOS.Application.Features.Users.Queries.GetList;

public sealed class GetListUserQueryHandler(IUserRepository userRepository, IMapper mapper) : IRequestHandler<GetListUsersQuery, PaginatedListResponse<GetListUserResponse>>
{
    public async Task<PaginatedListResponse<GetListUserResponse>> Handle(GetListUsersQuery request, CancellationToken cancellationToken)
    {
        // ✅ Repository üzerinden paginated list alıyoruz (Clean Architecture - Application katmanı Persistence'a bağımlı değil)
        Paginate<User> userList = await userRepository.GetUsersAsync(
            index: request.PageRequest.PageIndex,
            size: request.PageRequest.PageSize,
            cancellationToken: cancellationToken);

        // ✅ AutoMapper ile DTO'ya map ediyoruz
        PaginatedListResponse<GetListUserResponse> response = mapper.Map<PaginatedListResponse<GetListUserResponse>>(userList);
        return response;
    }
}
