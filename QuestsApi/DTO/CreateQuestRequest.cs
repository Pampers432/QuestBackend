namespace QuestsApi.DTO;

public record CreateQuestRequest(
    string Title,
    string? Description,
    string Subject,
    string Difficulty,
    string Status,
    Guid? CategoryId,
    // Public — виден в общем каталоге, Private — только автору
    string Visibility,
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
    string? Attachment,
    int OrderIndex,
    List<CreateAnswerOptionRequest> AnswerOptions
);

public record CreateAnswerOptionRequest(
    string? Text,
    bool? IsCorrect,
    string? Attachment,
    int OrderIndex
);
