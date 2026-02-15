using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Attempt
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid QuestSessionId { get; set; }

    public string Status { get; set; } = null!;

    public int Score { get; set; }

    public int MaxScore { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public virtual QuestSession QuestSession { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
