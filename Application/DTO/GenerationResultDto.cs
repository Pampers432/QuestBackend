using System;
using System.Collections.Generic;

namespace Application.DTO
{
    public record GenerationResultDto(
        bool Success,
        GeneratedQuestDto? Quest,
        List<string> Warnings
    );

    public record GeneratedQuestDto(
        string Title,
        string Subject,
        string Difficulty,
        int TotalRooms,
        int TotalQuestions,
        int TotalPoints,
        List<GeneratedRoomDto> Rooms,
        double AvgDifficulty,
        string ValidationSummary
    );

    public record GeneratedRoomDto(
        Guid RoomTemplateId,
        string RoomTemplateName,
        string? Title,
        int OrderIndex,
        int QuestionsCount,
        List<GeneratedQuestionDto> Questions
    );

    public record GeneratedQuestionDto(
        string Type,
        string Text,
        int Points,
        string? Hint,
        string? TargetObject,
        int OrderIndex,
        int DifficultyRating,
        List<string>? Tags,
        List<GeneratedAnswerOptionDto> AnswerOptions
    );

    public record GeneratedAnswerOptionDto(
        string? Text,
        bool IsCorrect,
        int OrderIndex
    );
}