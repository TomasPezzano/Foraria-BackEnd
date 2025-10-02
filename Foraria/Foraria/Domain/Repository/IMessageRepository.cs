namespace Foraria.Domain.Repository
{
    using ForariaDomain;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    namespace Foraria.Domain.Repository
    {
        public interface IMessageRepository
        {
            Task<Message> Add(Message message);
            Task<Message?> GetById(int id);
            Task<IEnumerable<Message>> GetByThread(int threadId);
            Task Delete(Message message);
        }
    }
}
