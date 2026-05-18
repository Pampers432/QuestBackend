using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QuestsApi.Data;

namespace Data.Repositories;

public class RegistrationTokenRepository
{
    private readonly QuestPlatformContext _context;

    public RegistrationTokenRepository(QuestPlatformContext context)
    {
        _context = context;
    }

    public async Task<RegistrationToken> AddAsync(RegistrationToken token)
    {
        token.CreatedAt = DateTime.UtcNow;
        await _context.RegistrationTokens.AddAsync(token);
        await _context.SaveChangesAsync();
        return token;
    }

    public async Task<RegistrationToken?> GetByTokenAsync(string token)
    {
        return await _context.RegistrationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task<List<RegistrationToken>> GetExpiredAsync()
    {
        return await _context.RegistrationTokens
            .Where(t => t.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<int> DeleteExpiredAsync()
    {
        var expiredTokens = await GetExpiredAsync();
        _context.RegistrationTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();
        return expiredTokens.Count;
    }

    public async Task DeleteAsync(RegistrationToken token)
    {
        _context.RegistrationTokens.Remove(token);
        await _context.SaveChangesAsync();
    }

    public async Task MarkAsUsedAsync(RegistrationToken token)
    {
        token.IsUsed = true;
        await _context.SaveChangesAsync();
    }
}
