namespace Application.DTO;

public record GenerateQuestRequest(
    string Prompt,
    string Subject,
    string Difficulty,
    int RoomCount
);
