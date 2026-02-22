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

        public async Task<List<QuestDto>> GetAllQuestsAsync()
        {
            var quests = await _questRepository.GetAllQuestsAsync();

            return quests.Select(q => new QuestDto(
                Id: q.Id,
                Title: q.Title,
                Description: q.Description,
                Subject: q.Subject,
                Difficulty: q.Difficulty,
                Status: q.Status,
                AuthorId: q.AuthorId,
                Author: new UserDto(
                    Id: q.Author.Id,
                    Username: q.Author.Username,
                    Role: q.Author.Role,
                    IsBlocked: q.Author.IsBlocked
                ),
                QuestRooms: q.QuestRooms.Select(r => new QuestRoomDto(
                    Id: r.Id,
                    QuestId: r.QuestId,
                    RoomTemplateId: r.RoomTemplateId,
                    Title: r.Title,
                    OrderIndex: r.OrderIndex,
                    Questions: r.Questions.Select(qs => new QuestionDto(
                        Id: qs.Id,
                        QuestRoomId: qs.QuestRoomId,
                        TargetObject: qs.TargetObject,
                        Type: qs.Type,
                        Text: qs.Text,
                        Attachment: qs.Attachment,
                        Points: qs.Points,
                        Hint: qs.Hint,
                        OrderIndex: qs.OrderIndex,
                        AnswerOptions: qs.AnswerOptions.Select(a => new AnswerOptionDto(
                            Id: a.Id,
                            QuestionId: a.QuestionId,
                            Text: a.Text,
                            IsCorrect: a.IsCorrect,
                            OrderIndex: a.OrderIndex,
                            MatchPair: a.MatchPair,
                            SequenceOrder: a.SequenceOrder
                        )).ToList()
                    )).ToList(),
                    RoomTemplate: new RoomTemplateDto(
                        Id: r.RoomTemplate.Id,
                        Name: r.RoomTemplate.Name,
                        PreviewImageUrl: r.RoomTemplate.PreviewImage,
                        SceneData: r.RoomTemplate.SceneData
                    )
                )).ToList()
            )).ToList();
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
