using AutoMapper;
using Application.Models;
using Domain.Entities;
using Presentation.Contracts;

namespace Presentation.Infrastructure;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Event, EventDto>();
        CreateMap<CreateEventRequest, EventCreateDto>().ReverseMap();
        CreateMap<UpdateEventRequest, EventCreateDto>().ReverseMap();
    }
}
