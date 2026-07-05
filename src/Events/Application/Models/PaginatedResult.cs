namespace Application.Models;

public record PaginatedResult<T> where T : class
{
    public required IEnumerable<T> Items { get; set; }
    public int Total { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
}
