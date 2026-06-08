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

        private const string VisibilityPublic = "Public";
        private const string VisibilityPrivate = "Private";

        private IQueryable<Quest> BuildQuestBaseQuery(bool includeDetails)
        {
            IQueryable<Quest> query = _context.Quests
                .AsNoTracking()
                .Include(q => q.Author)
                .Include(q => q.Category);

            if (includeDetails)
            {
                query = query
                    .Include(q => q.QuestRooms)
                        .ThenInclude(qr => qr.RoomTemplate)
                    .Include(q => q.QuestRooms)
                        .ThenInclude(qr => qr.Questions)
                            .ThenInclude(qe => qe.AnswerOptions);
            }

            return query;
        }

        public async Task<List<Quest>> GetAllQuestsAsync()
        {
            return await BuildQuestBaseQuery(includeDetails: true)
                .Where(q => q.Visibility == VisibilityPublic)
                .ToListAsync();
        }

        public async Task<List<Quest>> GetLatestQuestsAsync(int count)
        {
            return await BuildQuestBaseQuery(includeDetails: true)
                .Where(q => q.Visibility == VisibilityPublic)
                .OrderByDescending(q => q.Id)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Quest>> SearchQuestsAsync(string? searchTerm, Guid? categoryId, string? categoryName)
        {
            var query = BuildQuestBaseQuery(includeDetails: true)
                .Where(q => q.Visibility == VisibilityPublic);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(q =>
                    q.Title.ToLower().Contains(term) ||
                    (q.Description != null && q.Description.ToLower().Contains(term)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(q => q.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                var nameTerm = categoryName.Trim().ToLower();
                query = query.Where(q => q.Category != null && q.Category.Name.ToLower().Contains(nameTerm));
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

        public async Task<Quest?> GetQuestByIdAsync(Guid id)
        {
            // Public-only по умолчанию (общий каталог / публичные страницы)
            return await BuildQuestBaseQuery(includeDetails: true)
                .FirstOrDefaultAsync(q => q.Id == id && q.Visibility == VisibilityPublic);
        }

        public async Task<Quest?> GetQuestByIdForUserAsync(Guid id, Guid userId, bool isAdmin)
        {
            // Admin может смотреть любые, остальные — только автору
            if (isAdmin)
            {
                return await BuildQuestBaseQuery(includeDetails: true)
                    .FirstOrDefaultAsync(q => q.Id == id);
            }

            return await BuildQuestBaseQuery(includeDetails: true)
                .FirstOrDefaultAsync(q => q.Id == id && (q.Visibility == VisibilityPublic || q.AuthorId == userId));
        }

        public async Task<List<Quest>> GetQuestsByStatusAsync(string status)
        {
            return await BuildQuestBaseQuery(includeDetails: true)
                .Where(q => q.Visibility == VisibilityPublic && q.Status == status)
                .ToListAsync();
        }

        public async Task<List<Quest>> GetQuestsByAuthorAsync(Guid authorId)
        {
            // Returns ALL quests (Public and Private) authored by the user
            return await BuildQuestBaseQuery(includeDetails: true)
                .Where(q => q.AuthorId == authorId)
                .ToListAsync();
        }

        public async Task<bool> UpdateQuestAsync(Quest quest)
        {
            var existing = await _context.Quests
                .Include(q => q.QuestRooms)
                    .ThenInclude(qr => qr.Questions)
                        .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == quest.Id);

            if (existing == null)
                return false;

            existing.Title = quest.Title;
            existing.Description = quest.Description;
            existing.Subject = quest.Subject;
            existing.Difficulty = quest.Difficulty;
            existing.Status = quest.Status;
            existing.CategoryId = quest.CategoryId;

            var existingRooms = existing.QuestRooms.ToList();
            foreach (var room in existingRooms)
            {
                var questions = room.Questions.ToList();
                foreach (var question in questions)
                {
                    var answers = question.AnswerOptions.ToList();
                    _context.AnswerOptions.RemoveRange(answers);
                }
                _context.Questions.RemoveRange(questions);
            }
            _context.QuestRooms.RemoveRange(existingRooms);

            foreach (var room in quest.QuestRooms)
            {
                room.Id = Guid.NewGuid();
                room.QuestId = existing.Id;

                foreach (var question in room.Questions)
                {
                    question.Id = Guid.NewGuid();
                    question.QuestRoomId = room.Id;

                    foreach (var answer in question.AnswerOptions)
                    {
                        answer.Id = Guid.NewGuid();
                        answer.QuestionId = question.Id;
                    }
                }
            }

            existing.QuestRooms = quest.QuestRooms;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteQuestAsync(Guid id)
        {
            var quest = await _context.Quests
                .Include(q => q.QuestRooms)
                    .ThenInclude(qr => qr.Questions)
                        .ThenInclude(q => q.AnswerOptions)
                .Include(q => q.QuestRooms)
                    .ThenInclude(qr => qr.Questions)
                        .ThenInclude(q => q.UserAnswers)
                .Include(q => q.QuestSessions)
                    .ThenInclude(qs => qs.Attempts)
                        .ThenInclude(a => a.UserAnswers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quest == null)
                return false;

            foreach (var session in quest.QuestSessions)
            {
                foreach (var attempt in session.Attempts)
                {
                    _context.UserAnswers.RemoveRange(attempt.UserAnswers);
                }
                _context.Attempts.RemoveRange(session.Attempts);
            }
            _context.QuestSessions.RemoveRange(quest.QuestSessions);

            foreach (var room in quest.QuestRooms)
            {
                foreach (var question in room.Questions)
                {
                    _context.AnswerOptions.RemoveRange(question.AnswerOptions);
                    _context.UserAnswers.RemoveRange(question.UserAnswers);
                }
                _context.Questions.RemoveRange(room.Questions);
            }
            _context.QuestRooms.RemoveRange(quest.QuestRooms);

            _context.Quests.Remove(quest);
            await _context.SaveChangesAsync();
            return true;
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

        public async Task<List<QuestSession>> GetSessionsByQuestIdsAsync(List<Guid> questIds)
        {
            return await _context.QuestSessions
                .AsNoTracking()
                .Include(s => s.Quest)
                .Include(s => s.Attempts)
                    .ThenInclude(a => a.User)
                .Where(s => questIds.Contains(s.QuestId))
                .ToListAsync();
        }
    }
}
