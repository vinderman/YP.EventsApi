using System.ComponentModel.DataAnnotations;

namespace Yp.EventsApi.Services.Dto;

public class EventCreateDto
{
    [Required]
    [StringLength(50), MinLength(2)]
    public string Title { get; set; }
    public string? Description { get; set; }
    [Required]
    public DateTime StartAt { get; set; }
    [Required]
    public DateTime EndAt { get; set; }
}