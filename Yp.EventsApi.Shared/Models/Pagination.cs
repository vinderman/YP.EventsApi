namespace Yp.EventsApi.Shared.Models;

public record Pagination
{
    /// <summary>
    /// Номер страницы 
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// Количество элементов на странице
    /// </summary>
    public int PageSize { get; set; } = 10;
}