using System.ComponentModel.DataAnnotations;

namespace Yp.EventsApi.Services.Dto;

public class EventDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required DateTime StartAt { get; set; }
    public required DateTime EndAt { get; set; }
}