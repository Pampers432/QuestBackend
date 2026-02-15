using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class UserAnswer
{
    public Guid Id { get; set; }

    public Guid AttemptId { get; set; }

    public Guid QuestionId { get; set; }

    public string AnswerData { get; set; } = null!;

    public bool IsCorrect { get; set; }

    public int PointsAwarded { get; set; }

    public DateTime AnsweredAt { get; set; }

    public virtual Attempt Attempt { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;
}
