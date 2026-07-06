namespace Shared.Messages;

public class BookingConfirmed
{
    public Guid EventId { get; set; }
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
}