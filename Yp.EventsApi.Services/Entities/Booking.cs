using Yp.EventsApi.Shared.Enums;

namespace Yp.EventsApi.Services.Entities;

public class Booking
{
    private Booking()
    {
        
    }
    
    public static Booking CreateInstance(Guid id , Guid eventId, BookingStatus status)
    {
       
        return new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = status,
            CreatedAt = DateTime.UtcNow,
        };
    }
    public Guid Id { get; set; }
    
    public Guid EventId { get; set; }
    
    public BookingStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
    
    public Event Event { get; set; }
}