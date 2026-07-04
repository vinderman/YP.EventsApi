using Yp.EventsApi.Domain.Enums;

namespace Yp.EventsApi.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateJwtToken(Guid userId, UserRole role);
}