using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain;

public class NotificationPreference
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [ForeignKey("User")]
    public int UserId { get; set; }
    public User User { get; set; }
    public bool PushEnabled { get; set; } = true;
    public bool EmailEnabled { get; set; } = false;
    public bool SmsEnabled { get; set; } = false;
    public bool ExpenseNotificationsEnabled { get; set; } = true;
    public bool MeetingNotificationsEnabled { get; set; } = true;
    public bool VotingNotificationsEnabled { get; set; } = false;
    public bool ForumNotificationsEnabled { get; set; } = true;
    public bool MaintenanceNotificationsEnabled { get; set; } = true;
    public bool ClaimNotificationsEnabled { get; set; } = true;
    [MaxLength(500)]
    public string? FcmToken { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsConfigured { get; set; } = false;
}
