using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain;

public class Notification
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    [Required]
    [MaxLength(50)]
    public string Type { get; set; }
    [Required]
    [MaxLength(200)]
    public string Title { get; set; }
    [Required]
    [MaxLength(1000)]
    public string Body { get; set; }
    [Required]
    [MaxLength(20)]
    public string Channel { get; set; }
    [Required]
    [MaxLength(20)]
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public int? RelatedEntityId { get; set; }
    [MaxLength(50)]
    public string? RelatedEntityType { get; set; }
    public string? MetadataJson { get; set; }
    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
}

public static class NotificationType
{
    public const string ExpenseReminder = "ExpenseReminder";
    public const string ExpenseCreated = "ExpenseCreated";
    public const string VotingAvailable = "VotingAvailable";
    public const string VotingClosingSoon = "VotingClosingSoon";
    public const string VotingClosed = "VotingClosed";
    public const string MeetingReminder = "MeetingReminder";
    public const string ClaimResponse = "ClaimResponse";
    public const string ForumActivity = "ForumActivity";
    public const string MaintenanceScheduled = "MaintenanceScheduled";
}

public static class NotificationStatus
{
    public const string Pending = "Pending";
    public const string Sent = "Sent";
    public const string Failed = "Failed";
    public const string Read = "Read";
}
public static class NotificationChannel
{
    public const string Push = "Push";
    public const string Email = "Email";
    public const string SMS = "SMS";
}