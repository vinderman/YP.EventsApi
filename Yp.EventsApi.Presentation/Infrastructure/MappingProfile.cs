using AutoMapper;
using Yp.EventsApi.Application.Models;
using Yp.EventsApi.Domain.Entities;
using Yp.EventsApi.Presentation.Contracts;

namespace Yp.EventsApi.Presentation.Infrastructure;

public class MappingProfile: Profile
{
    public MappingProfile()
    {
        CreateMap<Event, EventDto>();
        CreateMap<CreateEventRequest, EventCreateDto>().ReverseMap();
        CreateMap<UpdateEventRequest, EventCreateDto>().ReverseMap();
        CreateMap<Booking, BookingDto>().ReverseMap();
    }
}