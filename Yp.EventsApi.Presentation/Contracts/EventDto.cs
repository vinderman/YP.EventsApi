namespace Yp.EventsApi.Presentation.Contracts;

public class EventDto
{
    /// <summary>
    /// Идентификатор
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Название события
    /// </summary>
    public required string Title { get; set; }
    
    /// <summary>
    /// Описание события
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Дата начала события
    /// </summary>
    public required DateTime StartAt { get; set; }
    
    /// <summary>
    /// Дата окончания события
    /// </summary>
    public required DateTime EndAt { get; set; }
    
    /// <summary>
    /// Общее количество мест на событии
    /// </summary>
    public required int TotalSeats { get; set; }
    
    /// <summary>
    /// Общее количество мест на событии
    /// </summary>
    public required int AvailableSeats { get; set; }
}