using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Roles.GetRoleById;

public sealed class GetRoleByIdHandler
{
    private readonly LifeOSDbContext _context;

    public GetRoleByIdHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResult<GetRoleByIdResponse>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);

        if (role is null)
            return ApiResultExtensions.Failure<GetRoleByIdResponse>(ResponseMessages.Role.NotFound);

        var response = new GetRoleByIdResponse(role.Id, role.Name ?? string.Empty);
        return ApiResultExtensions.Success(response, "Rol bilgisi başarıyla getirildi");
    }
}

