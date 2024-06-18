using AutoMapper;
using Roadmap.Application.Dtos.Responses;
using Roadmap.Domain.Entities;

namespace Roadmap.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<Domain.Entities.Roadmap, RoadmapResponseDto>()
            .ForMember(x => x.StarsCount, o
                => o.MapFrom(x => x.Stared.Count()));
        CreateMap<Domain.Entities.Roadmap,RoadmapPagedDto>()
            .ForMember(x => x.StarsCount, o
                => o.MapFrom(x => x.Stared.Count()));
    }
}