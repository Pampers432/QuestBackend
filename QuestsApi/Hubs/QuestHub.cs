using Application.DTO;
using Microsoft.AspNetCore.SignalR;

namespace QuestsApi.Hubs
{
    /// <summary>
    /// SignalR хаб для real-time обновлений квестов и шаблонов.
    /// Позволяет серверу push-ить изменения всем подключенным клиентам.
    /// Регистрируется в Program.cs через MapHub("/questHub").
    /// </summary>
    public class QuestHub : Hub
    {
        /// <summary>
        /// Подписка на обновления всех квестов.
        /// Клиент вызывает этот метод при монтировании страницы с квестами.
        /// </summary>
        public async Task SubscribeQuests()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "quests");
            await Clients.Caller.SendAsync("SubscribedToQuests", "Connected to quests updates");
        }

        /// <summary>
        /// Подписка на обновления всех шаблонов.
        /// </summary>
        public async Task SubscribeTemplates()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "templates");
            await Clients.Caller.SendAsync("SubscribedToTemplates", "Connected to templates updates");
        }

        /// <summary>
        /// Подписка на обновления конкретного шаблона.
        /// </summary>
        public async Task SubscribeToTemplate(string templateId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"template-{templateId}");
            await Clients.Caller.SendAsync("SubscribedToTemplate", $"Connected to template {templateId}");
        }

        /// <summary>
        /// Отправка уведомления о созданном квесте.
        /// Вызывается из QuestService после успешного создания.
        /// </summary>
        public async Task QuestCreated(QuestDto quest)
        {
            await Clients.Group("quests").SendAsync("QuestCreated", quest);
        }

        /// <summary>
        /// Отправка уведомления об обновлённом квесте.
        /// </summary>
        public async Task QuestUpdated(QuestDto quest)
        {
            await Clients.Group("quests").SendAsync("QuestUpdated", quest);
        }

        /// <summary>
        /// Отправка уведомления об удалённом квесте.
        /// </summary>
        public async Task QuestDeleted(Guid questId)
        {
            await Clients.Group("quests").SendAsync("QuestDeleted", questId);
        }

        /// <summary>
        /// Отправка уведомления о новом шаблоне.
        /// </summary>
        public async Task TemplateCreated(RoomTemplateDto template)
        {
            await Clients.Group("templates").SendAsync("TemplateCreated", template);
        }

        /// <summary>
        /// Отправка уведомления об обновлённом шаблоне.
        /// </summary>
        public async Task TemplateUpdated(RoomTemplateDto template)
        {
            await Clients.Group("templates").SendAsync("TemplateUpdated", template);
        }

        /// <summary>
        /// Отправка уведомления об удалённом шаблоне.
        /// </summary>
        public async Task TemplateDeleted(Guid templateId)
        {
            await Clients.Group("templates").SendAsync("TemplateDeleted", templateId);
        }

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "all");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "all");
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Подписка на обновления сессий (результаты).
        /// </summary>
        public async Task SubscribeSessions()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "sessions");
            await Clients.Caller.SendAsync("SubscribedToSessions", "Connected to sessions updates");
        }

        /// <summary>
        /// Подписка на обновления конкретной сессии.
        /// </summary>
        public async Task SubscribeToSession(string sessionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"session-{sessionId}");
            await Clients.Caller.SendAsync("SubscribedToSession", $"Connected to session {sessionId}");
        }
    }
}
