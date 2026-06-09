namespace Application.DTO;

public record QuestSessionListItemDto(
    Guid Id,
    string QuestTitle,
    string AccessCode,
    DateTime StartsAt,
    DateTime? EndsAt,
    bool IsActive,
    int ParticipantCount
);
