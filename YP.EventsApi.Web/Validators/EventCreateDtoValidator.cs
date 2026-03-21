using FluentValidation;
using YP.EventApi.Web.Contracts;

namespace YP.EventApi.Web.Validators;

public class EventCreateDtoValidator: AbstractValidator<EventCreateDto>
{
    public EventCreateDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название события обязательно для заполнения")
            .MaximumLength(100).MinimumLength(2).WithMessage("Название события должно быть от 2 до 100 символов");

        RuleFor(x => x.StartAt).NotEmpty().WithMessage("Дата начала события обязательна для заполнения");
        
        RuleFor(x => x.EndAt)
            .NotEmpty().WithMessage("Дата окончания события обязательна для заполнения")
            .GreaterThan(x => x.StartAt).WithMessage("Дата окончания события не может быть раньше даты начала");
        
    }
}