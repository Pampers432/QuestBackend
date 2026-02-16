using Application.DTO;
using Domain.Entities;
using QuestsApi;
using System;
using System.Collections.Generic;

using System.Text;

namespace Application.Services
{
    public class QuestService
    {
        private readonly QuestRepository _questRepository;

        public QuestService(QuestRepository questRepository)
        {
            _questRepository = questRepository;
        }

        public async Task<string> CreateQuestAsync(Quest quest)
        {
            return await _questRepository.CreateQuestAsync(quest);
        }

        public async Task<bool> SaveTemplateAsync(RoomTemplate template)
        {
            var res = await _questRepository.SaveTemplateAsync(template);

            return res;
        }

        public async Task<List<RoomTemplate>> GetAllTemplatesAsync()
        {
            var res = await _questRepository.GetAllTemplatesAsync();

            return res;
        }
    }
}
