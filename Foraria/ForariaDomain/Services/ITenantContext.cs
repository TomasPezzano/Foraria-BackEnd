namespace ForariaDomain.Services;

public interface ITenantContext
{
    int? GetCurrentConsortiumIdOrNull();

    int GetCurrentConsortiumId();

    int GetCurrentUserId();
    bool HasAccessToConsortium(int consortiumId);
    List<int> GetUserConsortiumIds();
    bool HasActiveTenant();
}