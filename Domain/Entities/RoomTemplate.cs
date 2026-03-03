using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class RoomTemplate
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? PreviewImage { get; set; }

    public string SceneData { get; set; } = null!;

    public virtual ICollection<QuestRoom> QuestRooms { get; set; } = new List<QuestRoom>();
}
