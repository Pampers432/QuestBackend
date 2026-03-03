using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public partial class RoomTemplate
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    [NotMapped]
    public string? PreviewImageUrl
    {
        get => PreviewImage;
        set => PreviewImage = value;
    }

    public string? PreviewImage { get; set; }

    public string SceneData { get; set; } = null!;

    public virtual ICollection<QuestRoom> QuestRooms { get; set; } = new List<QuestRoom>();
}
