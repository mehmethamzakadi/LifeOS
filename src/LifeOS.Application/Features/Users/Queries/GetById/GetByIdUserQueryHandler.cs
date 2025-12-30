using AutoMapper;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.Queries.GetById;

public sealed class GetByIdUserQueryHandler(
    LifeOSDbContext context,
    IMapper mapper) : IRequestHandler<GetByIdUserQuery, IDataResult<GetByIdUserResponse>>
{
    public async Task<IDataResult<GetByIdUserResponse>> Handle(GetByIdUserQuery request, CancellationToken cancellationToken)
    {
        // ✅ Read-only sorgu - tracking'e gerek yok (performans için)
        User? user = await context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.Id && !u.IsDeleted, cancellationToken);

        if (user is null)
        {
            return new ErrorDataResult<GetByIdUserResponse>("Kullanıcı bulunamadı!");
        }

        GetByIdUserResponse userResponse = mapper.Map<GetByIdUserResponse>(user);
        return new SuccessDataResult<GetByIdUserResponse>(userResponse);
    }
}
