using Domain.Entities;
using QuestsApi.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Repositories
{
    public class AuthRepository
    {
        private readonly QuestPlatformContext _context;

        public AuthRepository(QuestPlatformContext context)
        {
            _context = context;
        }

        public async Task<string> CreateUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return "Успех";
        }
    }
}
