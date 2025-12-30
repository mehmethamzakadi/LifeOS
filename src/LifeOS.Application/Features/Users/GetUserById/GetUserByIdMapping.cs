using AutoMapper;
using LifeOS.Domain.Entities;

namespace LifeOS.Application.Features.Users.GetUserById;

/// <summary>
/// AutoMapper profile for GetUserById feature.
/// This mapping is specific to GetUserById slice and kept within the slice.
/// </summary>
public sealed class GetUserByIdMapping : Profile
{
    public GetUserByIdMapping()
    {
        CreateMap<User, GetUserByIdResponse>().ReverseMap();
    }
}

