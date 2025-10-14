﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foraria.Domain.Repository;
using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface IGetAllReserve
{
    Task<List<Reserve>> Execute();
}
public class GetAllReserve : IGetAllReserve
{
    private readonly IReserveRepository _reserveRepository;
    public GetAllReserve(IReserveRepository reserveRepository)
    {
        _reserveRepository = reserveRepository;
    }
    public async Task<List<Reserve>> Execute()
    {
       return await _reserveRepository.GetAll();
    }
}

