using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Persistence
{
    public class CallParticipantRepository : ICallParticipantRepository
    {
        private readonly ForariaContext _context;

        public CallParticipantRepository(ForariaContext context)
        {
            _context = context;
        }

        public void Add(CallParticipant participant)
        {
            _context.CallParticipants.Add(participant);
            _context.SaveChanges();
        }

        public IEnumerable<CallParticipant> GetParticipants(int callId)
        {
            return _context.CallParticipants
                .Where(x => x.CallId == callId)
                .AsNoTracking()
                .ToList();
        }

        public List<CallParticipant> GetByCallId(int callId)
        {
            return _context.CallParticipants
                .Where(p => p.CallId == callId)
                .AsNoTracking()
                .ToList();
        }

        public void Update(CallParticipant participant)
        {
            _context.CallParticipants.Update(participant);
            _context.SaveChanges();
        }

        public void SetMute(int callId, int userId, bool isMuted)
        {
            var participant = _context.CallParticipants
                .FirstOrDefault(p => p.CallId == callId && p.UserId == userId);

            if (participant == null)
                throw new NotFoundException("El participante no existe en esta llamada.");

            participant.IsMuted = isMuted;

            _context.SaveChanges();
        }

        public void SetCamera(int callId, int userId, bool isCameraOn)
        {
            var participant = _context.CallParticipants
                .FirstOrDefault(p => p.CallId == callId && p.UserId == userId);

            if (participant == null)
                throw new NotFoundException("El participante no existe en esta llamada.");

            participant.IsCameraOn = isCameraOn;

            _context.SaveChanges();
        }

        public void SetDisconnected(int callId, int userId)
        {
            var participant = _context.CallParticipants
                .FirstOrDefault(p => p.CallId == callId && p.UserId == userId);

            if (participant == null)
                throw new NotFoundException("El participante no existe en esta llamada.");

            participant.IsConnected = false;
            participant.LeftAt = DateTime.UtcNow;

            _context.SaveChanges();
        }
    }
}
