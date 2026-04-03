using Yp.EventsApi.Shared.Enums;

namespace Yp.EventsApi.Shared.Contracts;

public class UpdateBookingStatusRequest
{
    public Guid Id { get; set; }
    
    public BookingStatus Status { get; set; }
}