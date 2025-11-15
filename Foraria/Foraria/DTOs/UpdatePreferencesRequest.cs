namespace Foraria.DTOs;

public class UpdatePreferencesRequest
{
    public bool PushEnabled { get; set; }
    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }
    public bool ExpenseNotificationsEnabled { get; set; }
    public bool MeetingNotificationsEnabled { get; set; }
    public bool VotingNotificationsEnabled { get; set; }
    public bool ForumNotificationsEnabled { get; set; }
    public bool MaintenanceNotificationsEnabled { get; set; }
    public bool ClaimNotificationsEnabled { get; set; }
}
