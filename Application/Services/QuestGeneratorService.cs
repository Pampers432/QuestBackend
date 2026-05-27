using Application.DTO;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class QuestGeneratorService
    {
        private readonly QuestService _questService;
        private readonly Random _rng;

        public QuestGeneratorService(QuestService questService)
        {
            _questService = questService;
            _rng = new Random();
        }

        public async Task<GenerationResultDto> GenerateQuestAsync(GenerateQuestRequest request, Guid authorId)
        {
            var warnings = new List<string>();

            var templates = await LoadTemplatesAsync(request.RoomSlots);
            warnings.AddRange(templates.Item2);

            if (!templates.Item1.Any())
                return new(false, null, new List<string> { "No room templates could be loaded" });

            var selectedQuestions = SelectQuestionsForRooms(request, warnings);

            var quest = BuildQuest(request, authorId, templates.Item1, selectedQuestions);

            var validationWarnings = ValidateQuest(quest);
            warnings.AddRange(validationWarnings);

            var isSolvable = !validationWarnings.Any(w => w.StartsWith("CRITICAL:"));
            if (!isSolvable)
                return new(false, null, warnings);

            var resultDto = BuildResultDto(quest, warnings);
            return new(true, resultDto, warnings);
        }

        public async Task<Guid> GenerateAndSaveQuestAsync(GenerateQuestRequest request, Guid authorId)
        {
            var result = await GenerateQuestAsync(request, authorId);
            if (!result.Success || result.Quest == null)
                throw new InvalidOperationException(string.Join("; ", result.Warnings));

            var quest = new Quest
            {
                Id = Guid.NewGuid(),
                Title = result.Quest.Title,
                Description = request.Description,
                Subject = result.Quest.Subject,
                Difficulty = result.Quest.Difficulty,
                AuthorId = authorId,
                Status = "Draft",
                CategoryId = request.CategoryId,
                QuestRooms = new List<QuestRoom>()
            };

            foreach (var roomDto in result.Quest.Rooms)
            {
                var room = new QuestRoom
                {
                    Id = Guid.NewGuid(),
                    QuestId = quest.Id,
                    RoomTemplateId = roomDto.RoomTemplateId,
                    Title = roomDto.Title,
                    OrderIndex = roomDto.OrderIndex,
                    Questions = roomDto.Questions.Select(q => new Question
                    {
                        Id = Guid.NewGuid(),
                        QuestRoomId = Guid.Empty,
                        Type = q.Type,
                        Text = q.Text,
                        Points = q.Points,
                        Hint = q.Hint,
                        TargetObject = q.TargetObject,
                        OrderIndex = q.OrderIndex,
                        AnswerOptions = q.AnswerOptions.Select(a => new AnswerOption
                        {
                            Id = Guid.NewGuid(),
                            QuestionId = Guid.Empty,
                            Text = a.Text,
                            IsCorrect = a.IsCorrect,
                            OrderIndex = a.OrderIndex
                        }).ToList()
                    }).ToList()
                };
                quest.QuestRooms.Add(room);
            }

            foreach (var room in quest.QuestRooms)
                foreach (var q in room.Questions)
                {
                    q.QuestRoomId = room.Id;
                    foreach (var a in q.AnswerOptions)
                        a.QuestionId = q.Id;
                }

            await _questService.CreateQuestAsync(quest);
            return quest.Id;
        }

        private async Task<(List<RoomTemplate>, List<string>)> LoadTemplatesAsync(List<RoomSlotConfig> roomSlots)
        {
            var templates = new List<RoomTemplate>();
            var warnings = new List<string>();
            var uniqueIds = roomSlots.Select(s => s.RoomTemplateId).Distinct();

            foreach (var id in uniqueIds)
            {
                var template = await _questService.GetTemplateByIdAsync(id);
                if (template != null) templates.Add(template);
                else warnings.Add($"Room template {id} not found");
            }

            return (templates, warnings);
        }

        private List<List<QuestionPoolEntry>> SelectQuestionsForRooms(GenerateQuestRequest request, List<string> warnings)
        {
            var result = new List<List<QuestionPoolEntry>>();
            var usedIndices = new HashSet<int>();
            var pool = request.QuestionPool;

            foreach (var slot in request.RoomSlots)
            {
                var candidates = new List<(QuestionPoolEntry entry, int index)>();
                for (int i = 0; i < pool.Count; i++)
                {
                    if (!request.Config.AllowQuestionReuse && usedIndices.Contains(i)) continue;
                    candidates.Add((pool[i], i));
                }

                if (!candidates.Any()) candidates = pool.Select((e, i) => (e, i)).ToList();

                var selected = candidates
                    .OrderByDescending(x => x.entry.Weight)
                    .Take(slot.QuestionsCount)
                    .Select(x => { usedIndices.Add(x.index); return x.entry; })
                    .ToList();

                if (selected.Count < slot.QuestionsCount)
                    warnings.Add($"Only {selected.Count} questions selected for room, requested {slot.QuestionsCount}");

                result.Add(selected);
            }

            return result;
        }

        private Quest BuildQuest(GenerateQuestRequest request, Guid authorId, List<RoomTemplate> templates, List<List<QuestionPoolEntry>> questionsPerRoom)
        {
            var quest = new Quest
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                Subject = request.Subject,
                Difficulty = request.Difficulty,
                AuthorId = authorId,
                Status = "Draft",
                CategoryId = request.CategoryId,
                QuestRooms = new List<QuestRoom>()
            };

            for (int i = 0; i < request.RoomSlots.Count; i++)
            {
                var slot = request.RoomSlots[i];
                var template = templates.FirstOrDefault(t => t.Id == slot.RoomTemplateId) ?? templates.FirstOrDefault();
                var questions = questionsPerRoom[i];

                var room = new QuestRoom
                {
                    Id = Guid.NewGuid(),
                    QuestId = quest.Id,
                    RoomTemplateId = slot.RoomTemplateId,
                    Title = slot.Title ?? template?.Name,
                    OrderIndex = i,
                    Questions = questions.Select((q, qi) => new Question
                    {
                        Id = Guid.NewGuid(),
                        QuestRoomId = Guid.Empty,
                        Type = q.Type,
                        Text = q.Text,
                        Points = q.Points,
                        Hint = q.Hint,
                        TargetObject = q.TargetZoneName,
                        OrderIndex = qi,
                        AnswerOptions = q.AnswerOptions.Select((a, ai) => new AnswerOption
                        {
                            Id = Guid.NewGuid(),
                            QuestionId = Guid.Empty,
                            Text = a.Text,
                            IsCorrect = a.IsCorrect,
                            OrderIndex = ai
                        }).ToList()
                    }).ToList()
                };
                quest.QuestRooms.Add(room);
            }

            return quest;
        }

        private List<string> ValidateQuest(Quest quest)
        {
            var warnings = new List<string>();

            if (quest.QuestRooms.Count == 0)
                warnings.Add("CRITICAL: Quest has no rooms");

            foreach (var room in quest.QuestRooms)
            {
                if (room.Questions.Count == 0)
                    warnings.Add($"CRITICAL: Room '{room.Title}' has no questions");

                foreach (var q in room.Questions)
                {
                    if (string.IsNullOrWhiteSpace(q.Text))
                        warnings.Add($"CRITICAL: Question in room '{room.Title}' has empty text");

                    if (!q.AnswerOptions.Any())
                        warnings.Add($"CRITICAL: Question '{q.Text}' has no answer options");

                    if ((q.Type == "single_choice" || q.Type == "multiple_choice") && !q.AnswerOptions.Any(a => a.IsCorrect == true))
                        warnings.Add($"CRITICAL: Question '{q.Text}' has no correct answer");

                    if (q.Points <= 0)
                        warnings.Add($"Question '{q.Text}' has zero or negative points");
                }
            }

            return warnings;
        }

        private GeneratedQuestDto BuildResultDto(Quest quest, List<string> warnings)
        {
            return new GeneratedQuestDto(
                Title: quest.Title,
                Subject: quest.Subject,
                Difficulty: quest.Difficulty,
                TotalRooms: quest.QuestRooms.Count,
                TotalQuestions: quest.QuestRooms.Sum(r => r.Questions.Count),
                TotalPoints: quest.QuestRooms.Sum(r => r.Questions.Sum(q => q.Points)),
                Rooms: quest.QuestRooms.Select(r => new GeneratedRoomDto(
                    RoomTemplateId: r.RoomTemplateId,
                    RoomTemplateName: "",
                    Title: r.Title,
                    OrderIndex: r.OrderIndex,
                    QuestionsCount: r.Questions.Count,
                    Questions: r.Questions.Select(q => new GeneratedQuestionDto(
                        Type: q.Type,
                        Text: q.Text,
                        Points: q.Points,
                        Hint: q.Hint,
                        TargetObject: q.TargetObject,
                        OrderIndex: q.OrderIndex,
                        DifficultyRating: 5,
                        Tags: null,
                        AnswerOptions: q.AnswerOptions.Select(a => new GeneratedAnswerOptionDto(
                            Text: a.Text,
                            IsCorrect: a.IsCorrect ?? false,
                            OrderIndex: a.OrderIndex
                        )).ToList()
                    )).ToList()
                )).ToList(),
                AvgDifficulty: 5.0,
                ValidationSummary: string.Join("; ", warnings)
            );
        }
    }
}