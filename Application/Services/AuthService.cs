using Data.Repositories;
using Domain.Entities;
using System.Security.Cryptography;

namespace Application.Services
{
    public class AuthService
    {
        private readonly AuthRepository _repository;

        public AuthService(AuthRepository repository)
        {
            _repository = repository;
        }

        public async Task<(bool Success, string Message, User? User)> RegisterAsync(string username, string password)
        {
            var normalizedUsername = username.Trim();
            if (string.IsNullOrWhiteSpace(normalizedUsername) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Логин и пароль обязательны", null);
            }

            var existingUser = await _repository.GetByUsernameAsync(normalizedUsername);
            if (existingUser is not null)
            {
                return (false, "Пользователь уже существует", null);
            }

            var user = new User
            {
                Username = normalizedUsername,
                PasswordHash = HashPassword(password),
                IsBlocked = false,
                Role = "student"
            };

            var createdUser = await _repository.CreateUserAsync(user);
            return (true, "Успех", createdUser);
        }

        public async Task<(bool Success, string Message, User? User)> LoginAsync(string username, string password)
        {
            var normalizedUsername = username.Trim();
            var user = await _repository.GetByUsernameAsync(normalizedUsername);
            if (user is null)
            {
                return (false, "Неверный логин или пароль", null);
            }

            if (user.IsBlocked)
            {
                return (false, "Пользователь заблокирован", null);
            }

            if (!VerifyPassword(password, user.PasswordHash))
            {
                return (false, "Неверный логин или пароль", null);
            }

            return (true, "Успех", user);
        }

        private static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        private static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 2)
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[0]);
            var expectedHash = Convert.FromBase64String(parts[1]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}
