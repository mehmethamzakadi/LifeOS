using AutoMapper;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.GetUserById;

public sealed class GetUserByIdHandler
{
    private readonly LifeOSDbContext _context;
    private readonly IMapper _mapper;

    public GetUserByIdHandler(LifeOSDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ApiResult<GetUserByIdResponse>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);

        if (user is null)
            return ApiResultExtensions.Failure<GetUserByIdResponse>("Kullanıcı bulunamadı!");

        var response = _mapper.Map<GetUserByIdResponse>(user);
        return ApiResultExtensions.Success(response, "Kullanıcı bilgisi başarıyla getirildi");
    }
}

