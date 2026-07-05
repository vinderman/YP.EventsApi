using Shared.Messages;

namespace Application.Interfaces;

public interface ICreateBookingProducer
{
    public Task Produce(string topic, CreateBooking message);
}