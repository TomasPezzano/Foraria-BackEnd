using System.Collections.Generic;

namespace ForariaDomain.Repository;

public interface ICallRepository
{
    Call Create(Call call);
    Call? GetById(int id);
    void Update(Call call);
    List<Call> GetActiveCalls();
    List<Call> GetByConsortium(int consortiumId);

}
