using System.ComponentModel.DataAnnotations;

namespace YP.EventApi.Web.Contracts;

public class EventCreateDto
{
    [Required(ErrorMessage = "Название события обязательно для заполнения")]
    [StringLength(100, MinimumLength = 2,
        ErrorMessage = "Названия события должно быть от 2 до 100 символов")]
    public string Title { get; set; }
    
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Дата начала события обязательна для заполнения")]
    public DateTime? StartAt { get; set; }
    
    [Required(ErrorMessage = "Дата окончания события обязательна для заполнения")]
    public DateTime? EndAt { get; set; }
}