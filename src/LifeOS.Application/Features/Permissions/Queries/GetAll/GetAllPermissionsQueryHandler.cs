using LifeOS.Domain.Common.Results;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Permissions.Queries.GetAll;

public class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, IDataResult<GetAllPermissionsResponse>>
{
    private readonly LifeOSDbContext _context;

    public GetAllPermissionsQueryHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<IDataResult<GetAllPermissionsResponse>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
    {
        // Tüm permission'ları al
        var permissions = await _context.Permissions
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Type)
            .ToListAsync(cancellationToken);

        var grouped = permissions
            .GroupBy(p => p.Module)
            .Select(g => new PermissionModuleDto
            {
                ModuleName = g.Key,
                Permissions = g.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description ?? string.Empty,
                    Type = p.Type
                }).ToList()
            })
            .OrderBy(m => m.ModuleName)
            .ToList();

        var response = new GetAllPermissionsResponse
        {
            Modules = grouped
        };

        return new SuccessDataResult<GetAllPermissionsResponse>(response, "Permission'lar başarıyla getirildi");
    }
}
