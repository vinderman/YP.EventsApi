namespace Yp.EventsApi.Shared.Enums;

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
    Rejected
}