using Application.Interfaces;
using Domain.Entities;
using Shared.Domain.Enums;
using Shared.Exceptions;
using Shared.UnitOfWork;

namespace Application.Services.UserService;

public class UserService: IUserService
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IPasswordHasher passwordHasher, IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator, IUnitOfWork unitOfWork)
    {
        _passwordHasher = passwordHasher;
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
    }
    public async Task<bool> Register(string login, string password, UserRole? role, CancellationToken ct)
    {
        var userRole = role ?? UserRole.User;

        var hashedPassword = _passwordHasher.HashPassword(password);
        
        var existingUser = await _userRepository.GetUserByLogin(login, ct);
        if (existingUser != null)
        {
            throw new DomainValidationException("Текущий логин уже занят");
        }
        
        var user = User.CreateInstance(Guid.NewGuid(), login, hashedPassword, userRole);
        await _userRepository.CreateUser(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<string> Login(string login, string password, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByLogin(login, cancellationToken);

        if (user == null)
        {
            throw new EntityNotFoundException("Введены неверные учетные данные.");
        }

        var isPasswordMatch = _passwordHasher.VerifyPassword(password, user.PasswordHash);
        if (!isPasswordMatch)
        {
            throw new EntityNotFoundException("Введены неверные учетные данные.");
        }
        
        
        return _jwtTokenGenerator.GenerateJwtToken(user.Id, user.Role);
    }
}