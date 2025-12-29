using LifeOS.Application.Features.Users.Commands.ChangePassword;
using LifeOS.Application.Features.Users.Commands.UpdateCurrentUserProfile;
using LifeOS.Application.Features.Users.Queries.GetCurrentUserProfile;
using LifeOS.Domain.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LifeOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ProfileController(IMediator mediator) : BaseApiController(mediator)
{
    /// <summary>
    /// Kullanıcının kendi profil bilgilerini getirir
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var response = await Mediator.Send(new GetCurrentUserProfileQuery(), cancellationToken);
        return ToResponse(response);
    }

    /// <summary>
    /// Kullanıcının kendi profil bilgilerini günceller
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateCurrentUserProfileCommand command, CancellationToken cancellationToken)
    {
        var response = await Mediator.Send(command, cancellationToken);
        return ToResponse(response);
    }

    /// <summary>
    /// Kullanıcının şifresini değiştirir
    /// </summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var response = await Mediator.Send(command, cancellationToken);
        return ToResponse(response);
    }
}
