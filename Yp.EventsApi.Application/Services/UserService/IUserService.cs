using Yp.EventsApi.Domain.Enums;

namespace Yp.EventsApi.Application.Services.UserService;

public interface IUserService
{
    Task<bool> Register(string login, string password, UserRole? role, CancellationToken cancellationToken);
    
    Task<string> Login(string login, string password, CancellationToken cancellationToken);
}