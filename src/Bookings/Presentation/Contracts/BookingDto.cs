using Domain.Enums;

namespace Presentation.Contracts;

public class BookingDto
{
    public Guid Id { get; set; }
    
    public Guid EventId { get; set; }
    
    public BookingStatus Status { get; set; }

    public DateTime? ProcessedAt { get; set; }
}
