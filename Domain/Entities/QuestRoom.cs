using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class QuestRoom
{
    public Guid Id { get; set; }

    public Guid QuestId { get; set; }

    public Guid RoomTemplateId { get; set; }

    public string? Title { get; set; }

    public int OrderIndex { get; set; }

    public virtual Quest Quest { get; set; } = null!;

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual RoomTemplate RoomTemplate { get; set; } = null!;
}
