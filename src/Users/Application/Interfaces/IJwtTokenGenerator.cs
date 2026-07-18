using Shared.Domain.Enums;

namespace Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateJwtToken(Guid userId, UserRole role);
}