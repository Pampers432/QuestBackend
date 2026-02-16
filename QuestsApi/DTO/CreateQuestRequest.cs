namespace QuestsApi.DTO;

public record CreateQuestRequest(
    string Title,
    string? Description,
    string Subject,
    string Difficulty,
    string Status,
    List<CreateQuestRoomRequest> Rooms
);

public record CreateQuestRoomRequest(
    Guid RoomTemplateId,
    string? Title,
    int OrderIndex,
    List<CreateQuestionRequest> Questions
);

public record CreateQuestionRequest(
    string Text,
    string Type,
    int Points,
    string? Hint,
    int OrderIndex,
    List<CreateAnswerOptionRequest> AnswerOptions
);

public record CreateAnswerOptionRequest(
    string? Text,
    bool? IsCorrect,
    int OrderIndex
);