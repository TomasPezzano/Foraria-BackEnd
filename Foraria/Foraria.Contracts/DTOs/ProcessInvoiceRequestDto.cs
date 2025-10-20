using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foraria.Contracts.DTOs;

public class ProcessInvoiceRequestDto
{
    public IFormFile File { get; set; } = null!;
}
