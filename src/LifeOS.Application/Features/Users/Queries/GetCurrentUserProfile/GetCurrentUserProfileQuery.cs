using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Users.Queries.GetCurrentUserProfile;

public sealed record GetCurrentUserProfileQuery() : IRequest<IDataResult<GetCurrentUserProfileResponse>>;
