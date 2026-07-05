// =============================================================================
// ЗАКОММЕНТИРОВАНО: исключение будет выбрасываться при проверке события
// через Kafka-ответ от сервиса Events (событие уже началось).
// =============================================================================
/*
namespace Shared.Exceptions;

public class BookingEventException: Exception
{
    private const string DefaultMessage = "Невозможно создать бронь для прошедшего события";
    
    public BookingEventException(string? message) : base(DefaultMessage)
    {
    }
}
*/
