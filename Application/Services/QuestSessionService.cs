using Domain.Entities;
using QuestsApi;
using QuestsApi.DTO;
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
