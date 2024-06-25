using System.Text.Json;
using AutoMapper;
using Roadmap.Application.Dtos.Responses;
using Roadmap.Domain.Entities;

namespace Roadmap.Application.Mapping;

public class JsonDocumentToStringConverter : IValueConverter<JsonDocument?, JsonDocument?>
{
    public JsonDocument? Convert(JsonDocument? sourceMember, ResolutionContext context)
    {
        return sourceMember; 
    }
}

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<Domain.Entities.Roadmap, RoadmapResponseDto>()
            .ForMember(dest => dest.Content, opt => opt.ConvertUsing<JsonDocumentToStringConverter, JsonDocument?>());
        CreateMap<Domain.Entities.Roadmap, RoadmapPagedDto>();
    }
}
