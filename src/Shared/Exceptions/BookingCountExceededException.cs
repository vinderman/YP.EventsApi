namespace Shared.Exceptions;

public class BookingCountExceededException: Exception
{
    private const string DefaultMessage = "Превышено максимально количество бронирований";

    public BookingCountExceededException() : base(DefaultMessage)
    {
    }

    public BookingCountExceededException(string? message) : base(message)
    {
    }
}
