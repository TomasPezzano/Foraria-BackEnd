namespace Foraria.DTOs
{
    public class CallStartRequestDto
    {
        public int CreatedByUserId { get; set; }
        public List<int> Participants { get; set; } = new();
    }

}
