

namespace Foraria.DTOs
{
    public class PollWithResultsDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime DeletedAt { get; set; }
        public string State { get; set; }
        public int CategoryPollId { get; set; }
        public List<PollOptionDto> PollOptions { get; set; } = new List<PollOptionDto>();
        public List<PollResultDto> PollResults { get; set; } = new List<PollResultDto>();
    }

}
