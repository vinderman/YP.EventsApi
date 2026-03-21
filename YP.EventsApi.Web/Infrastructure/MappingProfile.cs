using AutoMapper;
using Yp.EventsApi.Services.Entities;
using Yp.EventsApi.Shared.Contracts;

namespace YP.EventApi.Web.Infrastructure;

public class MappingProfile: Profile
{
    public MappingProfile()
    {
        CreateMap<Event, EventDto>().ReverseMap();
        CreateMap<Event, EventCreateDto>().ReverseMap();
    }
}