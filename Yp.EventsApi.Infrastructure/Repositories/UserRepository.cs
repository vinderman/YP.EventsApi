using Microsoft.EntityFrameworkCore;
using Yp.EventsApi.Application.Interfaces;
using Yp.EventsApi.Application.Services.UserService;
using Yp.EventsApi.Domain.Entities;

namespace Yp.EventsApi.Infrastructure.Repositories;

public class UserRepository: IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task CreateUser(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task<User?> GetUserByLogin(string login, CancellationToken cancellationToken)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Login == login, cancellationToken);
    }
}