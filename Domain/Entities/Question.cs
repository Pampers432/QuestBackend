using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Question
{
    public Guid Id { get; set; }

    public Guid QuestRoomId { get; set; }

    public string? TargetObject { get; set; }

    public string Type { get; set; } = null!;

    public string Text { get; set; } = null!;

    public string? Attachment { get; set; }

    public int Points { get; set; }

    public string? Hint { get; set; }

    public int OrderIndex { get; set; }

    public virtual ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();

    public virtual QuestRoom QuestRoom { get; set; } = null!;

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
