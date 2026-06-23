
using Moq;
using Yp.EventsApi.Application.Interfaces;
using Yp.EventsApi.Application.Models;
using Yp.EventsApi.Application.Services.EventService;
using Yp.EventsApi.Domain.Entities;

namespace Yp.EventsApi.Tests.EventServiceTests;

public class EventServiceAddTests
{

    [Fact]
    public async Task Create_AddsEventAndSavesChanges()
    {
        var eventRepository = new Mock<IEventRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var service = new EventService(eventRepository.Object, unitOfWork.Object);

        var dto = new CreateEventRequest
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
