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

        public async Task<bool> SaveTemplateAsync(RoomTemplate template)
        {
            var res = await _questRepository.SaveTemplateAsync(template);

            return res;
        }
    }
}
