using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class TemplateRename
{
    public Guid Id { get; set; }

    public Guid TemplateId { get; set; }

    public Guid? UserId { get; set; }

    public string SystemKey { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual RoomTemplate Template { get; set; } = null!;

    public virtual User? User { get; set; }
}
