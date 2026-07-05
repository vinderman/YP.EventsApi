using Shared.Domain.Enums;

namespace Domain.Entities;

public class User
{
    private User()
    {
        
    }

    public static User CreateInstance(Guid userId, string login, string passwordHash, UserRole role)
    {
        return new User
        {
            Id = userId,
            Login = login,
            PasswordHash = passwordHash,
            Role = role
        };
    }
    
    public Guid Id { get; set; }
    
    public string Login { get; set; }
    
    public UserRole Role { get; set; }
    
    public string PasswordHash { get; set; }
}