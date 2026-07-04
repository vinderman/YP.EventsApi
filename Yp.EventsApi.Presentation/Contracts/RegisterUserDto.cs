using Yp.EventsApi.Domain.Enums;

namespace Yp.EventsApi.Presentation.Contracts;

public record RegisterUserDto(string login, string password, UserRole? Role);