namespace Application.DTO;

public record AuthorAnalyticsDto(
    int TotalQuests,
    int TotalSessions,
    int TotalAttempts,
    int CompletedAttempts,
    double? AverageScorePercent,
    List<AttemptInfoDto> RecentAttempts
);

public record AttemptInfoDto(
    Guid AttemptId,
    string Username,
    string Status,
    int Score,
    int MaxScore,
    DateTime StartedAt,
    DateTime? FinishedAt,
    string QuestTitle
);
