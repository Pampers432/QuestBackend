using Domain.Entities;
using QuestsApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class QuestSessionService
    {
        private readonly QuestRepository _questRepository;

        public QuestSessionService(QuestRepository questRepository)
        {
            _questRepository = questRepository;
        }

        public async Task<QuestSession> CreateSessionAsync(QuestSession session)
        {
            await _questRepository.CreateSessionAsync(session); 

            return session;
        }
    }
}
