namespace Foraria.DTOs
{
    public class UserDocumentStatsDto
    {
        public int TotalUserDocuments { get; set; }
        public int TotalConsortiumDocuments { get; set; }
        public int TotalCombined { get; set; }
        public Dictionary<string, int> DocumentsByCategory { get; set; }
        public DateTime? LastUploadDate { get; set; }
    }
}
