using Domain.Entities;

namespace Presentation.Contracts;

public static class BookingMapper
{
    public static BookingDto ToDto(this Booking booking)
    {
        return new BookingDto
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status,
            ProcessedAt = booking.ProcessedAt
        };
    }
}
