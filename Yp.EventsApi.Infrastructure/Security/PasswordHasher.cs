using System.Security.Cryptography;
using System.Text;
using Yp.EventsApi.Application.Interfaces;

namespace Yp.EventsApi.Infrastructure.Security;

public class PasswordHasher: IPasswordHasher
{
    public string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes); 
    }

    public bool VerifyPassword(string plainPassword, string hashedPassword)
    {
        var passwordToCompare = HashPassword(plainPassword);
        
        return passwordToCompare == hashedPassword;
    }
}