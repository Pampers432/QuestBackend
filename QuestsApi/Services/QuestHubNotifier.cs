using Application.DTO;
using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using QuestsApi.Hubs;

namespace QuestsApi.Services
{
    /// <summary>
    /// Реализация IQuestNotifier с использованием SignalR.
    /// </summary>
    public class QuestHubNotifier : IQuestNotifier
    {
        private readonly IHubContext<QuestHub> _hubContext;

        public QuestHubNotifier(IHubContext<QuestHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyQuestCreatedAsync(QuestDto quest)
        {
            await _hubContext.Clients.Group("quests").SendAsync("QuestCreated", quest);
        }

        public async Task NotifyQuestUpdatedAsync(QuestDto quest)
        {
            await _hubContext.Clients.Group("quests").SendAsync("QuestUpdated", quest);
        }

        public async Task NotifyQuestDeletedAsync(Guid questId)
        {
            await _hubContext.Clients.Group("quests").SendAsync("QuestDeleted", questId);
        }

        public async Task NotifyTemplateCreatedAsync(RoomTemplateDto template)
        {
            await _hubContext.Clients.Group("templates").SendAsync("TemplateCreated", template);
        }

        public async Task NotifyTemplateUpdatedAsync(RoomTemplateDto template)
        {
            await _hubContext.Clients.Group("templates").SendAsync("TemplateUpdated", template);
        }

        public async Task NotifyTemplateDeletedAsync(Guid templateId)
        {
            await _hubContext.Clients.Group("templates").SendAsync("TemplateDeleted", templateId);
        }

        public async Task NotifySessionUpdatedAsync(Guid sessionId)
        {
            // Уведомляем конкретную сессию
            await _hubContext.Clients.Group($"session-{sessionId}").SendAsync("SessionUpdated", sessionId);
        }
    }
}
