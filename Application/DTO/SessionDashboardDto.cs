namespace Application.DTO;

public record SessionDashboardDto(
    Guid SessionId,
    string AccessCode,
    string QuestTitle,
    DateTime StartsAt,
    DateTime? EndsAt,
    bool IsActive,
    int TotalAttempts,
    int CompletedAttempts,
    List<AttemptSummaryDto> Attempts
);

public record AttemptSummaryDto(
    Guid AttemptId,
    Guid UserId,
    string Username,
    string Status,
    int Score,
    int MaxScore,
    DateTime StartedAt,
    DateTime? FinishedAt
);

