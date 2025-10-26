using Foraria.Domain.Repository;
using ForariaDomain;

namespace Foraria.Application.UseCase
{
    public class ToggleReaction
    {
        private readonly IReactionRepository _repository;

        public ToggleReaction(IReactionRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Execute(int userId, int? messageId, int? threadId, int reactionType)
        {
            var existing = await _repository.GetByUserAndTarget(userId, messageId, threadId);

            if (existing != null)
            {
                if (existing.ReactionType == reactionType)
                {
                    await _repository.Remove(existing);
                    return false;
                }
                else
                {
                    existing.ReactionType = reactionType;
                    await _repository.Update(existing);
                    return true;
                }
            }
            else
            {
                var reaction = new Reaction
                {
                    User_id = userId,
                    Message_id = messageId,
                    Thread_id = threadId,
                    ReactionType = reactionType
                };

                await _repository.Add(reaction);
                return true;
            }
        }
    }
}
