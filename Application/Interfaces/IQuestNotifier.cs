using Application.DTO;

namespace Application.Interfaces
{
    /// <summary>
    /// Интерфейс для отправки real-time уведомлений.
    /// Позволяет отделить Application слой от инфраструктуры SignalR.
    /// </summary>
    public interface IQuestNotifier
    {
        Task NotifyQuestCreatedAsync(QuestDto quest);
        Task NotifyQuestUpdatedAsync(QuestDto quest);
        Task NotifyQuestDeletedAsync(Guid questId);
        Task NotifyTemplateCreatedAsync(RoomTemplateDto template);
        Task NotifyTemplateUpdatedAsync(RoomTemplateDto template);
        Task NotifyTemplateDeletedAsync(Guid templateId);
        Task NotifySessionUpdatedAsync(Guid sessionId);
    }
}
