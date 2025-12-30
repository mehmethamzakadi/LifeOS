using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Roles.Queries.GetRoleById;

public sealed class GetRoleByIdQueryHandler(LifeOSDbContext context) : IRequestHandler<GetRoleByIdRequest, IDataResult<GetRoleByIdQueryResponse>>
{
    public async Task<IDataResult<GetRoleByIdQueryResponse>> Handle(GetRoleByIdRequest request, CancellationToken cancellationToken)
    {
        // ✅ Read-only sorgu - tracking'e gerek yok (performans için)
        Role? role = await context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken);

        if (role is null)
        {
            return new ErrorDataResult<GetRoleByIdQueryResponse>("Rol bulunamadı!");
        }

        GetRoleByIdQueryResponse result = new(Id: role.Id, Name: role.Name!);
        return new SuccessDataResult<GetRoleByIdQueryResponse>(result);
    }
}
