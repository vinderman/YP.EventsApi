using Moq;
using Yp.EventsApi.Services.Entities;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Services.Interfaces;
using Yp.EventsApi.Services.Services.EventService;
using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Tests.Common;

namespace Yp.EventsApi.Tests.EventServiceTests;

public class EventServiceUpdateTests
{
    private readonly IMapper _mapper = ServiceTestFactory.CreateMapper();

    [Fact]
    public async Task Update_UpdatesExistingEventAndSavesChanges()
    {
        var eventId = Guid.NewGuid();
        var existing = Event.CreateInstance(eventId, "Old", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 5);
        var dto = new EventCreateDto
        {
            Title = "New title",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(2),
            TotalSeats = 10
        };

        var eventRepository = new Mock<IEventRepository>();
        eventRepository
            .Setup(r => r.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var unitOfWork = new Mock<IUnitOfWork>();
        var service = new EventService(_mapper, eventRepository.Object, unitOfWork.Object);

        var result = await service.Update(eventId, dto, CancellationToken.None);

        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(eventId, result.Id);
        Assert.Equal(dto.Title, result.Title);
    }

    [Fact]
    public async Task Update_ThrowsEntityNotFoundException_WhenEventMissing()
    {
        var eventRepository = new Mock<IEventRepository>();
        eventRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);

        var service = new EventService(_mapper, eventRepository.Object, Mock.Of<IUnitOfWork>());
        var dto = new EventCreateDto
        {
            Title = "Test",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow,
            TotalSeats = 1
        };

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => service.Update(Guid.NewGuid(), dto, CancellationToken.None));
    }
}
