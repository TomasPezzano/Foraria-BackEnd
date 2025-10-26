using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Repository;

public interface IConsortiumRepository
{
    Task<Consortium> FindById(int id);
}
