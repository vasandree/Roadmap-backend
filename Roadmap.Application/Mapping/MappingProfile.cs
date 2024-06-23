using AutoMapper;
using Roadmap.Application.Dtos.Responses;
using Roadmap.Domain.Entities;

namespace Roadmap.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<Domain.Entities.Roadmap, RoadmapResponseDto>();
        CreateMap<Domain.Entities.Roadmap, RoadmapPagedDto>();
    }
}