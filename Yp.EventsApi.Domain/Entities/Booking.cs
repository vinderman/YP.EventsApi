using Yp.EventsApi.Domain.Enums;
using Yp.EventsApi.Domain.Exceptions;

namespace Yp.EventsApi.Domain.Entities;

public class Booking
{
    private Booking()
    {
        
    }
    
    public static Booking CreateInstance(Guid id , Guid eventId, BookingStatus status, Guid userId)
    {
       
        return new Booking
        {
            Id = id,
            EventId = eventId,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };
    }

    public void CancelBooking(Booking booking)
    {
        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new DomainValidationException("Нельзя повторно отменить бронирование");
        }
    }
    public Guid Id { get; set; }
    
    public Guid EventId { get; set; }
    
    public BookingStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
    
    public Guid UserId { get; set; }
    
    public User User { get; set; }
    
    public Event Event { get; set; }
    
}