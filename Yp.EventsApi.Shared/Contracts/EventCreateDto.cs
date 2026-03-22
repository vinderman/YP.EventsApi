namespace Yp.EventsApi.Shared.Contracts;

public class EventCreateDto
{
    public string Title { get; set; }
    
    public string? Description { get; set; }
    
    public DateTime StartAt { get; set; }
    
    public DateTime EndAt { get; set; }
}