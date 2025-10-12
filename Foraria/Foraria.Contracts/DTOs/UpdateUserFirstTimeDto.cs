﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foraria.Contracts.DTOs;

public class UpdateUserFirstTimeRequestDto
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmNewPassword { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Dni { get; set; }
    public IFormFile? Photo { get; set; }
}
public class UpdateUserFirstTimeResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string? Token { get; set; }  
    public string? RefreshToken { get; set; }
}
