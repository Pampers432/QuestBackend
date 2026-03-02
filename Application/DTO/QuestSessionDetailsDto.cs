using System;
using System.Collections.Generic;

namespace Application.DTO
{
    public record QuestSessionAttemptUserAnswerDto(
        Guid Id,
        Guid AttemptId,
        Guid QuestionId,
        string AnswerData,
        bool IsCorrect,
        int PointsAwarded,
        DateTime AnsweredAt
    );

    public record QuestSessionAttemptDto(
        Guid Id,
        Guid UserId,
        string Username,
        string Status,
        int Score,
        int MaxScore,
        DateTime StartedAt,
        DateTime? FinishedAt,
        List<QuestSessionAttemptUserAnswerDto> UserAnswers
    );

    public record QuestSessionDetailsDto(
        Guid Id,
        Guid QuestId,
        Guid StartedBy,
        string StartedByUsername,
        int? TimeLimit,
        bool AllowPartialCompletion,
        bool AllowToSkip,
        string AccessCode,
        bool IsActive,
        DateTime StartsAt,
        DateTime? EndsAt,
        QuestDto Quest,
        List<QuestSessionAttemptDto> Attempts
    );
}
