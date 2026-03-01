using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QuestsApi.Data;

namespace Data.Repositories
{
    public class AuthRepository
    {
        private readonly QuestPlatformContext _context;

        public AuthRepository(QuestPlatformContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }
    }
}
