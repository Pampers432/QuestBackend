using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool IsBlocked { get; set; }

    public virtual ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();

    public virtual ICollection<QuestSession> QuestSessions { get; set; } = new List<QuestSession>();

    public virtual ICollection<Quest> Quests { get; set; } = new List<Quest>();
}
