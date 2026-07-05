namespace Application.Models;

public record UpdateEventRequest
{
    /// <summary>
    /// Название события
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Описание события
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Дата начала события
    /// </summary>
    public DateTime StartAt { get; set; }

    /// <summary>
    /// Дата окончания события
    /// </summary>
    public DateTime EndAt { get; set; }

    /// <summary>
    /// Общее количество мест на событии
    /// </summary>
    public int TotalSeats { get; set; }
}
