using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Quest
{
    public Guid Id { get; set; }

    // Видимость контента в каталогах/доступе
    // "Public" — виден в общем каталоге
    // "Private" — виден только автору
    public string Visibility { get; set; } = "Public";

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Subject { get; set; } = null!;

    public string Difficulty { get; set; } = null!;

    public Guid AuthorId { get; set; }

    public Guid? CategoryId { get; set; }

    public string Status { get; set; } = null!;

    public virtual User Author { get; set; } = null!;

    public virtual Category? Category { get; set; }

    public virtual ICollection<QuestRoom> QuestRooms { get; set; } = new List<QuestRoom>();

    public virtual ICollection<QuestSession> QuestSessions { get; set; } = new List<QuestSession>();
}
