using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface IUpdateOldReserves
{
    Task Execute();
}
public class UpdateOldReserves : IUpdateOldReserves
{
    private readonly IReserveRepository _reserveRepository;

    public UpdateOldReserves(IReserveRepository reserveRepository)
    {
        _reserveRepository = reserveRepository;
    }

    public async Task Execute()
    {
        var now = DateTime.Now;

        // Obtenemos todas las reservas
        var reserves = await _reserveRepository.GetAll();

        // Filtramos las que ya pasaron su hora de DeletedAt
        var oldReserves = reserves
            .Where(r => r.DeletedAt.HasValue && r.DeletedAt.Value <= now && r.State != "Viejo")
            .ToList();

        foreach (var reserve in oldReserves)
        {
            reserve.State = "Viejo";
        }

        if (oldReserves.Any())
        {
            await _reserveRepository.UpdateRange(oldReserves);
        }
    }
}
