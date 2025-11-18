namespace ForariaDomain.Models;

public class CreatePreferenceResponse
{
    public string PreferenceId { get; set; }

    public string InitPoint { get; set; }

    public CreatePreferenceResponse(string PreferenceId, string InitPoint)
    {
         this.PreferenceId = PreferenceId;
         this.InitPoint = InitPoint;
    }
}
