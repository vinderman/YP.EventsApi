using AutoMapper;
using Moq;
using Yp.EventsApi.Services.Entities;
using Yp.EventsApi.Services.Interfaces;
using Yp.EventsApi.Services.Services.EventService;
using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Tests.Common;

namespace Yp.EventsApi.Tests.EventServiceTests;

public class EventServiceAddTests
{
    private readonly IMapper _mapper = ServiceTestFactory.CreateMapper();

    [Fact]
    public async Task Create_AddsEventAndSavesChanges()
    {
        var eventRepository = new Mock<IEventRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var service = new EventService(_mapper, eventRepository.Object, unitOfWork.Object);

        var dto = new EventCreateDto
        {
            Title = "Test Event",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1),
            TotalSeats = 10
        };

        var result = await service.Create(dto, CancellationToken.None);

        eventRepository.Verify(r => r.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(dto.Title, result.Title);
    }
}
