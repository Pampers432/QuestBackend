using Application.DTO;
using Domain.Entities;
using QuestsApi;

namespace Application.Services;

public class QuestGeneratorService
{
    private readonly QuestRepository _questRepository;
    private readonly QuestService _questService;

    public QuestGeneratorService(QuestRepository questRepository, QuestService questService)
    {
        _questRepository = questRepository;
        _questService = questService;
    }

    public async Task<QuestDto> GenerateQuestAsync(GenerateQuestRequest request, Guid authorId)
    {
        var templates = await _questService.GetAllTemplatesAsync();
        var random = Random.Shared;

        var rooms = new List<QuestRoom>();
        for (int i = 0; i < request.RoomCount; i++)
        {
            var template = templates.Count > 0
                ? templates[random.Next(templates.Count)]
                : null;

            var room = new QuestRoom
            {
                Id = Guid.NewGuid(),
                Title = $"Комната {i + 1}",
                OrderIndex = i,
                RoomTemplateId = template?.Id ?? Guid.NewGuid(),
                Questions = new List<Question>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Text = $"Вопрос {i + 1}: {request.Prompt}",
                        Type = "single",
                        Points = 10,
                        OrderIndex = 0,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { Id = Guid.NewGuid(), Text = "Вариант A", IsCorrect = true, OrderIndex = 0 },
                            new() { Id = Guid.NewGuid(), Text = "Вариант B", IsCorrect = false, OrderIndex = 1 },
                        }
                    }
                }
            };
            rooms.Add(room);
        }

        var quest = new Quest
        {
            Id = Guid.NewGuid(),
            Title = $"Сгенерированный квест: {request.Prompt}",
            Description = $"Автоматически сгенерированный квест на тему: {request.Prompt}",
            Subject = request.Subject,
            Difficulty = request.Difficulty,
            Status = "draft",
            Visibility = "Private",
            AuthorId = authorId,
            QuestRooms = rooms
        };

        return MapToDto(quest);
    }

    public async Task<Guid> GenerateAndSaveQuestAsync(GenerateQuestRequest request, Guid authorId)
    {
        var template = new RoomTemplate
        {
            Id = Guid.NewGuid(),
            Name = $"Шаблон: {request.Prompt}",
            SceneData = "[]",
            PreviewImage = null
        };
        await _questRepository.SaveTemplateAsync(template);

        var quest = new Quest
        {
            Id = Guid.NewGuid(),
            Title = $"Сгенерированный квест: {request.Prompt}",
            Description = $"Автоматически сгенерированный квест на тему: {request.Prompt}",
            Subject = request.Subject,
            Difficulty = request.Difficulty,
            Status = "draft",
            Visibility = "Private",
            AuthorId = authorId,
            QuestRooms = new List<QuestRoom>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    RoomTemplateId = template.Id,
                    Title = "Комната 1",
                    OrderIndex = 0,
                    Questions = new List<Question>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Text = $"Вопрос: {request.Prompt}",
                            Type = "single",
                            Points = 10,
                            OrderIndex = 0,
                            AnswerOptions = new List<AnswerOption>
                            {
                                new() { Id = Guid.NewGuid(), Text = "Вариант A", IsCorrect = true, OrderIndex = 0 },
                                new() { Id = Guid.NewGuid(), Text = "Вариант B", IsCorrect = false, OrderIndex = 1 },
                            }
                        }
                    }
                }
            }
        };

        await _questRepository.CreateQuestAsync(quest);
        return quest.Id;
    }

    private static QuestDto MapToDto(Quest quest)
    {
        return new QuestDto(
            Id: quest.Id,
            Title: quest.Title,
            Description: quest.Description,
            Subject: quest.Subject,
            Difficulty: quest.Difficulty,
            Status: quest.Status,
            Visibility: quest.Visibility,
            AuthorId: quest.AuthorId,
            Author: new UserDto(Guid.Empty, "generated", "User", false),
            CategoryId: quest.CategoryId,
            Category: null,
            QuestRooms: quest.QuestRooms.Select(r => new QuestRoomDto(
                Id: r.Id,
                QuestId: quest.Id,
                RoomTemplateId: r.RoomTemplateId,
                Title: r.Title,
                OrderIndex: r.OrderIndex,
                Questions: r.Questions.Select(q => new QuestionDto(
                    Id: q.Id,
                    QuestRoomId: r.Id,
                    TargetObject: q.TargetObject,
                    Type: q.Type,
                    Text: q.Text,
                    Attachment: q.Attachment,
                    Points: q.Points,
                    Hint: q.Hint,
                    OrderIndex: q.OrderIndex,
                    AnswerOptions: q.AnswerOptions.Select(a => new AnswerOptionDto(
                        Id: a.Id,
                        QuestionId: q.Id,
                        Text: a.Text,
                        IsCorrect: a.IsCorrect,
                        OrderIndex: a.OrderIndex,
                        MatchPair: a.MatchPair,
                        SequenceOrder: a.SequenceOrder
                    )).ToList()
                )).ToList(),
                RoomTemplate: new RoomTemplateDto(
                    Id: r.RoomTemplateId,
                    Name: "Generated Template",
                    PreviewImageUrl: null,
                    SceneData: "[]"
                )
            )).ToList()
        );
    }
}
