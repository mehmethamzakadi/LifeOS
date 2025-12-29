using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.Queries.GetUserRoles;

public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, IDataResult<GetUserRolesResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public GetUserRolesQueryHandler(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<IDataResult<GetUserRolesResponse>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        // ✅ Read-only sorgu - tracking'e gerek yok (performans için)
        var user = await _userRepository.GetAsync(
            u => u.Id == request.UserId,
            include: q => q.Include(u => u.UserRoles).ThenInclude(ur => ur.Role),
            enableTracking: false,
            cancellationToken: cancellationToken);

        if (user == null)
        {
            return new ErrorDataResult<GetUserRolesResponse>("Kullanıcı bulunamadı");
        }

        var roleNames = await _userRepository.GetRolesAsync(user);
        var userRoles = new List<UserRoleDto>();

        foreach (var roleName in roleNames)
        {
            var role = await _roleRepository.FindByNameAsync(roleName);
            if (role != null)
            {
                userRoles.Add(new UserRoleDto
                {
                    Id = role.Id,
                    Name = role.Name ?? string.Empty
                });
            }
        }

        var response = new GetUserRolesResponse
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Roles = userRoles
        };

        return new SuccessDataResult<GetUserRolesResponse>(response, "Kullanıcı rolleri başarıyla getirildi");
    }
}
