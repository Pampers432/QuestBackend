using Application.DTO;
using Application.Interfaces;
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

        public async Task<List<QuestDto>> GetLatestQuestsAsync(int count = 5)
        {
            var quests = await _questRepository.GetLatestQuestsAsync(count);
            return MapQuestsToDto(quests);
        }

        public async Task<List<QuestDto>> SearchQuestsAsync(string? searchTerm, Guid? categoryId)
        {
            var quests = await _questRepository.SearchQuestsAsync(searchTerm, categoryId);
            return MapQuestsToDto(quests);
        }

        public async Task<List<QuestDto>> GetAllQuestsAsync()
        {
            var quests = await _questRepository.GetAllQuestsAsync();
            return MapQuestsToDto(quests);
        }

        public async Task<QuestDto?> GetQuestByIdAsync(Guid id)
        {
            var quest = await _questRepository.GetQuestByIdAsync(id);
            if (quest == null) return null;
            return MapQuestsToDto(new List<Quest> { quest }).FirstOrDefault();
        }

        public async Task<List<QuestDto>> GetQuestsByStatusAsync(string status)
        {
            var quests = await _questRepository.GetQuestsByStatusAsync(status);
            return MapQuestsToDto(quests);
        }

        public async Task<List<QuestDto>> GetQuestsByAuthorAsync(Guid authorId)
        {
            var quests = await _questRepository.GetQuestsByAuthorAsync(authorId);
            return MapQuestsToDto(quests);
        }

        public async Task<bool> UpdateQuestAsync(Quest quest)
        {
            return await _questRepository.UpdateQuestAsync(quest);
        }

        public async Task<bool> DeleteQuestAsync(Guid id)
        {
            return await _questRepository.DeleteQuestAsync(id);
        }

        private List<QuestDto> MapQuestsToDto(List<Quest> quests)
        {
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
                CategoryId: q.CategoryId,
                Category: q.Category != null ? new CategoryDto(
                    Id: q.Category.Id,
                    Name: q.Category.Name,
                    Description: q.Category.Description
                ) : null,
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


        public async Task<RoomTemplate?> GetTemplateByIdAsync(Guid id)
        {
            return await _questRepository.GetTemplateByIdAsync(id);
        }

        public async Task<RoomTemplate> CreateTemplateAsync(RoomTemplate template)
        {
            return await _questRepository.CreateTemplateAsync(template);
        }

        public async Task<RoomTemplate?> UpdateTemplateAsync(RoomTemplate template)
        {
            return await _questRepository.UpdateTemplateAsync(template);
        }

        public async Task<bool> DeleteTemplateAsync(Guid id)
        {
            return await _questRepository.DeleteTemplateAsync(id);
        }

        public async Task<List<RoomTemplate>> GetAllTemplatesAsync()
        {
            var res = await _questRepository.GetAllTemplatesAsync();

            return res;
        }
    }
}
