namespace Yp.EventsApi.Domain.Enums;

public enum BookingStatus
{
    /// <summary>
    /// Ожидается обработка
    /// </summary>
    Pending,
    
    /// <summary>
    /// Бронировние подтверждено
    /// </summary>
    Confirmed,
    
    /// <summary>
    /// Бронирование отклонено 
    /// </summary>
    Rejected,
    
    /// <summary>
    /// Бронирование отменено
    /// </summary>
    Cancelled
}