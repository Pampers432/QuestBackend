using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class AnswerOption
{
    public Guid Id { get; set; }

    public Guid QuestionId { get; set; }

    public string? Text { get; set; }

    public bool? IsCorrect { get; set; }

    public string? MatchPair { get; set; }

    public int? SequenceOrder { get; set; }

    public int OrderIndex { get; set; }

    public virtual Question Question { get; set; } = null!;
}
