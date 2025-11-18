using ForariaDomain.Exceptions;
using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase
{
    public class GetCallDetails
    {
        private readonly ICallRepository _callRepository;

        public GetCallDetails(ICallRepository callRepository)
        {
            _callRepository = callRepository;
        }

        public Call Execute(int callId)
        {
            var call = _callRepository.GetById(callId);

            if (call == null)
                throw new NotFoundException("La llamada no existe.");

            return call;
        }
    }
}
