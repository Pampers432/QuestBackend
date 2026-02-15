using Domain.Entities;
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
    }
}
