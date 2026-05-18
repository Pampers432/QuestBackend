using System;

namespace Domain.Entities;

public class RegistrationToken
{
    public Guid Id { get; set; }

    public string Token { get; set; } = null!;

    public string Email { get; set; } = null!;

    public Guid UserId { get; set; }

    public bool IsUsed { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
