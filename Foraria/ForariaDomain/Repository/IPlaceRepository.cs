using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Repository;

public interface IPlaceRepository
{
    Task<Place?> GetById(int id);
}
