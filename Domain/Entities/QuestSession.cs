using System;
using System.Collections.Generic;

namespace Domain.Entities;

public class QuestSession
{
    public Guid Id { get; set; }

    public Guid QuestId { get; set; }

    public Guid StartedBy { get; set; }

    public int? TimeLimit { get; set; }

    public bool AllowPartialCompletion { get; set; }

    public bool AllowToSkip { get; set; }

    public string AccessCode { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime StartsAt { get; set; }

    public DateTime? EndsAt { get; set; }

    public virtual ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();

    public virtual Quest Quest { get; set; } = null!;

    public virtual User StartedByNavigation { get; set; } = null!;
}
