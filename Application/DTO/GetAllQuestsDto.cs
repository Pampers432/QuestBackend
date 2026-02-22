using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTO
{
    public record UserDto(
        Guid Id,
        string Username,
        string Role,
        bool IsBlocked
    );

    public record AnswerOptionDto(
        Guid Id,
        Guid QuestionId,
        string? Text,
        bool? IsCorrect,
        int OrderIndex,
        string? MatchPair,
        int? SequenceOrder
    );

    public record QuestionDto(
        Guid Id,
        Guid QuestRoomId,
        string? TargetObject,
        string Type,
        string Text,
        string? Attachment,
        int Points,
        string? Hint,
        int OrderIndex,
        List<AnswerOptionDto> AnswerOptions
    );

    public record QuestRoomDto(
        Guid Id,
        Guid QuestId,
        Guid RoomTemplateId,
        string? Title,
        int OrderIndex,
        List<QuestionDto> Questions,
        RoomTemplateDto RoomTemplate
    );

    public record QuestDto(
        Guid Id,
        string Title,
        string? Description,
        string Subject,
        string Difficulty,
        string Status,
        Guid AuthorId,
        UserDto Author,
        List<QuestRoomDto> QuestRooms
    );

    public record RoomTemplateDto(
    Guid Id,
    string Name,
    string? PreviewImageUrl,
    string SceneData
    );

}
