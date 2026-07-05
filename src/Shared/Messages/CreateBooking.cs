namespace Shared.Messages;

public class CreateBooking
{
    public Guid EventId { get; set; }
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
}