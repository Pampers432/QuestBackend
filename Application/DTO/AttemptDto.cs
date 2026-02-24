namespace QuestsApi.DTO
{
    public record StartAttemptDto(
        Guid UserId,
        Guid QuestSessionId
    );

    public record FinishAttemptDto(
        Guid AttemptId,
        int Score,
        int MaxScore,
        List<UserAnswerDto> Answers
    );

    public record UserAnswerDto
    (
        Guid AttemptId,
        Guid QuestionId,
        string AnswerData,
        bool IsCorrect,
        int PointsAwarded
    );
}
