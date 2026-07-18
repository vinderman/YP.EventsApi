namespace Application.Models;

public record EventFilter : Pagination
{
    /// <summary>
    /// Фильтрация по названию события
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Фильтрация по дате начала события
    /// </summary>
    public DateTime? From { get; set; }

    /// <summary>
    /// Фильтрация по дате окончания события
    /// </summary>
    public DateTime? To { get; set; }
}
