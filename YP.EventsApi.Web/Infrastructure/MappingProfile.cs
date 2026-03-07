using AutoMapper;
using Yp.EventsApi.Services.Dto;
using Yp.EventsApi.Services.Entities;

namespace YP.EventApi.Web.Infrastructure;

public class MappingProfile: Profile
{
    public MappingProfile()
    {
        CreateMap<Event, EventDto>().ReverseMap();
        CreateMap<Event, EventCreateDto>().ReverseMap();
    }
}