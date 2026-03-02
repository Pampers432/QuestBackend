using Application.DTO;
using Domain.Entities;
using QuestsApi;
using QuestsApi.DTO;

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

        public async Task<QuestSessionDetailsDto?> GetSessionAsync(Guid id)
        {
            var session = await _questRepository.GetSessionAsync(id);

            if (session == null)
            {
                return null;
            }

            return new QuestSessionDetailsDto(
                Id: session.Id,
                QuestId: session.QuestId,
                StartedBy: session.StartedBy,
                StartedByUsername: session.StartedByNavigation.Username,
                TimeLimit: session.TimeLimit,
                AllowPartialCompletion: session.AllowPartialCompletion,
                AllowToSkip: session.AllowToSkip,
                AccessCode: session.AccessCode,
                IsActive: session.EndsAt == null || session.EndsAt > DateTime.UtcNow,
                StartsAt: session.StartsAt,
                EndsAt: session.EndsAt,
                Quest: new QuestDto(
                    Id: session.Quest.Id,
                    Title: session.Quest.Title,
                    Description: session.Quest.Description,
                    Subject: session.Quest.Subject,
                    Difficulty: session.Quest.Difficulty,
                    Status: session.Quest.Status,
                    AuthorId: session.Quest.AuthorId,
                    Author: new UserDto(
                        Id: session.Quest.Author.Id,
                        Username: session.Quest.Author.Username,
                        Role: session.Quest.Author.Role,
                        IsBlocked: session.Quest.Author.IsBlocked
                    ),
                    QuestRooms: session.Quest.QuestRooms
                        .OrderBy(r => r.OrderIndex)
                        .Select(r => new QuestRoomDto(
                            Id: r.Id,
                            QuestId: r.QuestId,
                            RoomTemplateId: r.RoomTemplateId,
                            Title: r.Title,
                            OrderIndex: r.OrderIndex,
                            Questions: r.Questions
                                .OrderBy(q => q.OrderIndex)
                                .Select(q => new QuestionDto(
                                    Id: q.Id,
                                    QuestRoomId: q.QuestRoomId,
                                    TargetObject: q.TargetObject,
                                    Type: q.Type,
                                    Text: q.Text,
                                    Attachment: q.Attachment,
                                    Points: q.Points,
                                    Hint: q.Hint,
                                    OrderIndex: q.OrderIndex,
                                    AnswerOptions: q.AnswerOptions
                                        .OrderBy(a => a.OrderIndex)
                                        .Select(a => new AnswerOptionDto(
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
                ),
                Attempts: session.Attempts
                    .OrderByDescending(a => a.StartedAt)
                    .Select(a => new QuestSessionAttemptDto(
                        Id: a.Id,
                        UserId: a.UserId,
                        Username: a.User.Username,
                        Status: a.Status,
                        Score: a.Score,
                        MaxScore: a.MaxScore,
                        StartedAt: a.StartedAt,
                        FinishedAt: a.FinishedAt,
                        UserAnswers: a.UserAnswers
                            .Select(ua => new QuestSessionAttemptUserAnswerDto(
                                Id: ua.Id,
                                AttemptId: ua.AttemptId,
                                QuestionId: ua.QuestionId,
                                AnswerData: ua.AnswerData,
                                IsCorrect: ua.IsCorrect,
                                PointsAwarded: ua.PointsAwarded,
                                AnsweredAt: ua.AnsweredAt
                            )).ToList()
                    )).ToList()
            );
        }

        public async Task<Attempt> StartAttemptAsync(StartAttemptDto dto)
        {
            var attempt = new Attempt
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                QuestSessionId = dto.QuestSessionId,
                Status = "in_progress",
                StartedAt = DateTime.UtcNow,
                Score = 0,
                MaxScore = 0
            };

            await _questRepository.AddAttemptAsync(attempt);
            return attempt;
        }

        public async Task FinishAttemptAsync(Guid attemptId)
        {
            var attempt = await _questRepository.GetAttemptWithAnswersAsync(attemptId);

            if (attempt == null)
                throw new Exception("Attempt not found");

            attempt.Score = attempt.UserAnswers.Sum(a => a.PointsAwarded);
            attempt.MaxScore = attempt.UserAnswers.Sum(a => a.Question.Points);

            attempt.FinishedAt = DateTime.UtcNow;
            attempt.Status = "completed";

            await _questRepository.UpdateAttemptAsync(attempt);
        }

        public async Task SaveAnswerAsync(UserAnswerDto dto)
        {
            var answer = new UserAnswer
            {
                Id = Guid.NewGuid(),
                AttemptId = dto.AttemptId,
                QuestionId = dto.QuestionId,
                AnswerData = dto.AnswerData,
                IsCorrect = dto.IsCorrect,
                PointsAwarded = dto.PointsAwarded
            };

            await _questRepository.AddUserAnswerAsync(answer);
        }
    }
}
