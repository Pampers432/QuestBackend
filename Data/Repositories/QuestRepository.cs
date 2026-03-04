using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QuestsApi.Data;

namespace QuestsApi
{
    public class QuestRepository
    {
        private readonly QuestPlatformContext _context;

        public QuestRepository(QuestPlatformContext context)
        {
            _context = context;
        }

        public async Task<bool> SaveTemplateAsync(RoomTemplate temlpate)
        {
            await _context.RoomTemplates.AddAsync(new RoomTemplate { Name = temlpate.Name, PreviewImage = temlpate.PreviewImage, SceneData = temlpate.SceneData});
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<RoomTemplate>> GetAllTemplatesAsync()
        {     
            return await _context.RoomTemplates.AsNoTracking().ToListAsync();
        }


        public async Task<RoomTemplate?> GetTemplateByIdAsync(Guid id)
        {
            return await _context.RoomTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<RoomTemplate> CreateTemplateAsync(RoomTemplate template)
        {
            await _context.RoomTemplates.AddAsync(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<RoomTemplate?> UpdateTemplateAsync(RoomTemplate template)
        {
            var existing = await _context.RoomTemplates.FirstOrDefaultAsync(t => t.Id == template.Id);
            if (existing == null)
                return null;

            existing.Name = template.Name;
            existing.PreviewImage = template.PreviewImage;
            if (!string.IsNullOrWhiteSpace(template.SceneData))
            {
                existing.SceneData = template.SceneData;
            }

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteTemplateAsync(Guid id)
        {
            var template = await _context.RoomTemplates.FirstOrDefaultAsync(t => t.Id == id);
            if (template == null)
                return false;

            _context.RoomTemplates.Remove(template);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Quest>> GetAllQuestsAsync()
        {
            return await _context.Quests
                .AsNoTracking()
                .Include(q => q.Author)
                .Include(q => q.Category)
                .Include(q => q.QuestRooms)
                    .ThenInclude(qr => qr.RoomTemplate)
                .Include(q => q.QuestRooms)
                    .ThenInclude(qr => qr.Questions)
                        .ThenInclude(qe => qe.AnswerOptions)
                .ToListAsync();
        }

        public async Task<List<Quest>> GetLatestQuestsAsync(int count)
        {
            return await _context.Quests
                .AsNoTracking()
                .Include(q => q.Author)
                .Include(q => q.Category)
                .Include(q => q.QuestRooms)
                    .ThenInclude(qr => qr.RoomTemplate)
                .OrderByDescending(q => q.Id)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Quest>> SearchQuestsAsync(string? searchTerm, Guid? categoryId)
        {
            var query = _context.Quests
                .AsNoTracking()
                .Include(q => q.Author)
                .Include(q => q.Category)
                .Include(q => q.QuestRooms)
                    .ThenInclude(qr => qr.RoomTemplate)
                .Include(q => q.QuestRooms)
                    .ThenInclude(qr => qr.Questions)
                        .ThenInclude(qe => qe.AnswerOptions)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(q => q.Title.ToLower().Contains(term) || 
                                         (q.Description != null && q.Description.ToLower().Contains(term)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(q => q.CategoryId == categoryId.Value);
            }

            return await query.ToListAsync();
        }


        public async Task<string> CreateQuestAsync(Quest quest)
        {
            await _context.Quests.AddAsync(quest);
            await _context.SaveChangesAsync();

            return "Успех";
        }

        public async Task<QuestSession> CreateSessionAsync(QuestSession session)
        {
            await _context.QuestSessions.AddAsync(session);
            await _context.SaveChangesAsync();            

            return session;
        }

        public async Task<QuestSession?> GetByAccessCodeAsync(string accessCode)
        {
            return await _context.QuestSessions
                .AsNoTracking()
                .Include(s => s.StartedByNavigation)
                .Include(s => s.Quest)
                    .ThenInclude(q => q.Author)
                .Include(s => s.Quest)
                    .ThenInclude(q => q.Category)
                .Include(s => s.Quest)
                    .ThenInclude(q => q.QuestRooms)
                        .ThenInclude(qr => qr.RoomTemplate)
                .Include(s => s.Quest)
                    .ThenInclude(q => q.QuestRooms)
                        .ThenInclude(qr => qr.Questions)
                            .ThenInclude(qq => qq.AnswerOptions)
                .Include(s => s.Attempts)
                    .ThenInclude(a => a.User)
                .Include(s => s.Attempts)
                    .ThenInclude(a => a.UserAnswers)
                .FirstOrDefaultAsync(s => s.AccessCode == accessCode);
        }

        public async Task<QuestSession?> GetSessionByIdAsync(Guid sessionId)
        {
            return await _context.QuestSessions
                .AsNoTracking()
                .Include(s => s.Quest)
                .Include(s => s.Attempts)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public async Task<QuestSession?> GetSessionByIdWithDetailsAsync(Guid sessionId)
        {
            return await _context.QuestSessions
                .AsNoTracking()
                .Include(s => s.Quest)
                .Include(s => s.Attempts)
                    .ThenInclude(a => a.User)
                .Include(s => s.Attempts)
                    .ThenInclude(a => a.UserAnswers)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public async Task AddAttemptAsync(Attempt attempt)
        {
            await _context.Attempts.AddAsync(attempt);
            await _context.SaveChangesAsync();
        }

        public async Task<Attempt?> GetAttemptWithAnswersAsync(Guid attemptId)
        {
            return await _context.Attempts
                .Include(a => a.UserAnswers)
                .ThenInclude(ua => ua.Question)
                .FirstOrDefaultAsync(a => a.Id == attemptId);
        }

        public async Task UpdateAttemptAsync(Attempt attempt)
        {
            _context.Attempts.Update(attempt);
            await _context.SaveChangesAsync();
        }

        public async Task AddUserAnswerAsync(UserAnswer answer)
        {
            await _context.UserAnswers.AddAsync(answer);
            await _context.SaveChangesAsync();
        }
    }
}
