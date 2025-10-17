using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foraria.Domain.Repository;

namespace ForariaDomain.Application.UseCase;
public interface IGetUserById
{
    Task<User> Execute(int id);
}

public class GetUserById : IGetUserById
{
    public readonly IUserRepository _userRepository;


    public GetUserById(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> Execute(int id)
    {
        return await _userRepository.GetById(id);
    }
}
