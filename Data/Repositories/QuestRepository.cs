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

        //public async string SaveImage(string img)
        //{
        //    await _context.RoomTemplates.AddAsync(new RoomTemplate { Name = "test room", PreviewImage = img, SceneData = "test data" });
        //    await _context.SaveChangesAsync();
        //    return "Успех";
        //}

        public async Task<bool> SaveTemplateAsync(RoomTemplate temlpate)
        {
            await _context.RoomTemplates.AddAsync(new RoomTemplate { Name = temlpate.Name, PreviewImage = temlpate.PreviewImage, SceneData = temlpate.SceneData});
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<RoomTemplate>> GetAllTemplatesAsync()
        {     
            return await _context.RoomTemplates.ToListAsync();
        }

        public async Task<string> CreateQuestAsync(Quest quest)
        {
            await _context.Quests.AddAsync(quest);
            await _context.SaveChangesAsync();

            return "Успех";
        }
    }
}
