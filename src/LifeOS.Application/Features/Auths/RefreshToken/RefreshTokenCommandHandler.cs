using LifeOS.Application.Abstractions.Identity;
using LifeOS.Application.Features.Auths.Login;
using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Auths.RefreshToken;

public sealed class RefreshTokenCommandHandler(IAuthService authService) : IRequestHandler<RefreshTokenCommand, IDataResult<LoginResponse>>
{
    public async Task<IDataResult<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await authService.RefreshTokenAsync(request.RefreshToken);
    }
}
