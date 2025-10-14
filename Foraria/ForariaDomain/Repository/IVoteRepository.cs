﻿using Foraria.Application.UseCase;
using Foraria.Contracts.DTOs;
using ForariaDomain;

namespace Foraria.Domain.Repository
{
    public interface IVoteRepository
    {
        Task AddAsync(Vote vote);
        Task<Vote?> GetByUserAndPollAsync(int userId, int pollId);

        Task<IEnumerable<PollResultDto>> GetPollResultsAsync(int pollId);

    }
}
