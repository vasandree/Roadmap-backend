using AutoMapper;
using Roadmap.Application.Dtos.Responses;
using Roadmap.Domain.Entities;

namespace Roadmap.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UserDto, User>();

    }
}