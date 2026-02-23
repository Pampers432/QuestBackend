namespace QuestsApi.DTO
{
    public record CreateSessionDto(
     Guid QuestId,
     Guid StartedBy,
     int? TimeLimit,
     bool AllowPartialCompletion,
     bool AllowToSkip,
     string AccessCode,
     DateTime StartsAt,
     DateTime? EndsAt
 );
}
