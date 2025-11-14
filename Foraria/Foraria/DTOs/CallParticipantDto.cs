namespace Foraria.DTOs
{
    public class CallParticipantDto
    {
        public int UserId { get; set; }
        public bool IsMuted { get; set; }
        public bool IsCameraOn { get; set; }
        public bool IsConnected { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
    }

}
