using Domain.Entities;

namespace Application.Interfaces;

public interface IUserRepository
{
    Task CreateUser(User user);
    
    Task<User?> GetUserByLogin(string login, CancellationToken cancellationToken);
}