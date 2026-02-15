using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Quest
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Subject { get; set; } = null!;

    public string Difficulty { get; set; } = null!;

    public Guid AuthorId { get; set; }

    public string Status { get; set; } = null!;

    public virtual User Author { get; set; } = null!;

    public virtual ICollection<QuestRoom> QuestRooms { get; set; } = new List<QuestRoom>();

    public virtual ICollection<QuestSession> QuestSessions { get; set; } = new List<QuestSession>();
}
