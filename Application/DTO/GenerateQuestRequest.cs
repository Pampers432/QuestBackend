using System;
using System.Collections.Generic;

namespace Application.DTO
{
    public class GenerateQuestRequest
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string Subject { get; set; } = null!;
        public string Difficulty { get; set; } = null!;
        public Guid? CategoryId { get; set; }
        public List<RoomSlotConfig> RoomSlots { get; set; } = new();
        public List<QuestionPoolEntry> QuestionPool { get; set; } = new();
        public GenerationConfig Config { get; set; } = new();
    }

    public class RoomSlotConfig
    {
        public Guid RoomTemplateId { get; set; }
        public string? Title { get; set; }
        public List<string>? RequiredTags { get; set; }
        public int QuestionsCount { get; set; } = 3;
    }

    public class QuestionPoolEntry
    {
        public string Type { get; set; } = null!;
        public string Text { get; set; } = null!;
        public string? Hint { get; set; }
        public string? TargetZoneName { get; set; }
        public int Points { get; set; } = 10;
        public int DifficultyRating { get; set; } = 5;
        public List<string>? Tags { get; set; }
        public string? Theme { get; set; }
        public double Weight { get; set; } = 1.0;
        public List<AnswerOptionEntry> AnswerOptions { get; set; } = new();
    }

    public class AnswerOptionEntry
    {
        public string? Text { get; set; }
        public bool IsCorrect { get; set; }
        public string? MatchPair { get; set; }
        public int? SequenceOrder { get; set; }
    }

    public class GenerationConfig
    {
        public bool AllowQuestionReuse { get; set; } = false;
        public double DifficultyTolerance { get; set; } = 2.0;
        public bool EnforceZoneMatching { get; set; } = true;
        public string FallbackZoneName { get; set; } = "";
    }
}