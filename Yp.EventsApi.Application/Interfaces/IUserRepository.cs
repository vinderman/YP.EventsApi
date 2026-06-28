using Yp.EventsApi.Domain.Entities;

namespace Yp.EventsApi.Application.Interfaces;

public interface IUserRepository
{
    Task CreateUser(User user);
    
    Task<User?> GetUserByLogin(string login, CancellationToken cancellationToken);
}