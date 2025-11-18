namespace ForariaDomain;

public class PasswordResetToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public string CreatedByIp { get; set; } = string.Empty;
    public string? UsedByIp { get; set; }

    public User User { get; set; } = null!;

    public bool IsValid => !IsUsed && DateTime.Now < ExpiresAt;
}
