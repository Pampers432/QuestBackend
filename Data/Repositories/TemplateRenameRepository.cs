using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QuestsApi.Data;

namespace QuestsApi
{
    public class TemplateRenameRepository
    {
    private readonly QuestPlatformContext _context;

    public TemplateRenameRepository(QuestPlatformContext context)
    {
        _context = context;
    }

    public async Task<List<TemplateRename>> GetRenamesByTemplateAsync(Guid templateId, Guid? userId)
    {
        var query = _context.TemplateRenames
            .AsNoTracking()
            .Where(r => r.TemplateId == templateId);

        if (userId.HasValue)
        {
            query = query.Where(r => r.UserId == userId.Value);
        }
        else
        {
            // Global renames (UserId null)
            query = query.Where(r => r.UserId == null);
        }

        return await query.ToListAsync();
    }

    public async Task<List<TemplateRename>> GetRenamesForTemplateAndUserAsync(Guid templateId, Guid userId)
    {
        // Returns both global renames (UserId null) and user-specific renames
        return await _context.TemplateRenames
            .AsNoTracking()
            .Where(r => r.TemplateId == templateId && (r.UserId == userId || r.UserId == null))
            .ToListAsync();
    }

    public async Task UpsertRenameAsync(TemplateRename rename)
    {
        var existing = await _context.TemplateRenames
            .FirstOrDefaultAsync(r => r.TemplateId == rename.TemplateId
                                   && r.UserId == rename.UserId
                                   && r.SystemKey == rename.SystemKey);

        if (existing == null)
        {
            rename.Id = Guid.NewGuid();
            rename.CreatedAt = DateTime.UtcNow;
            rename.UpdatedAt = DateTime.UtcNow;
            await _context.TemplateRenames.AddAsync(rename);
        }
        else
        {
            existing.DisplayName = rename.DisplayName;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.TemplateRenames.Update(existing);
        }

        await _context.SaveChangesAsync();
    }

    public async Task BulkUpsertRenamesAsync(Guid templateId, Guid userId, List<(string SystemKey, string DisplayName)> renames)
    {
        foreach (var (systemKey, displayName) in renames)
        {
            var existing = await _context.TemplateRenames
                .FirstOrDefaultAsync(r => r.TemplateId == templateId
                                       && r.UserId == userId
                                       && r.SystemKey == systemKey);

            if (existing == null)
            {
                var newRename = new TemplateRename
                {
                    Id = Guid.NewGuid(),
                    TemplateId = templateId,
                    UserId = userId,
                    SystemKey = systemKey,
                    DisplayName = displayName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _context.TemplateRenames.AddAsync(newRename);
            }
            else
            {
                existing.DisplayName = displayName;
                existing.UpdatedAt = DateTime.UtcNow;
                _context.TemplateRenames.Update(existing);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteRenameAsync(Guid id)
    {
        var rename = await _context.TemplateRenames.FindAsync(id);
        if (rename != null)
        {
            _context.TemplateRenames.Remove(rename);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteRenamesByTemplateAsync(Guid templateId, Guid? userId)
    {
        var query = _context.TemplateRenames.Where(r => r.TemplateId == templateId);
        if (userId.HasValue)
        {
            query = query.Where(r => r.UserId == userId);
        }
        else
        {
            query = query.Where(r => r.UserId == null);
        }
        _context.TemplateRenames.RemoveRange(query);
        await _context.SaveChangesAsync();
    }
}
}


