using Shared.Domain.Enums;

namespace Presentation.Contracts;

public record RegisterUserDto(string login, string password, UserRole? Role);