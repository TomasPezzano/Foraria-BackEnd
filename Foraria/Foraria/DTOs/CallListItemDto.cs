namespace Foraria.DTOs
{
    public class CallListItemDto
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string? Description { get; set; }
        public string MeetingType { get; set; }
        public string Status { get; set; }

        public DateTime StartedAt { get; set; }

        public string Location { get; set; } = "Virtual";

        public int ParticipantsCount { get; set; }
    }

}
