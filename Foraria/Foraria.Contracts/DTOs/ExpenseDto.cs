﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foraria.Contracts.DTOs;

public class ExpenseDto
{
    public int ConsortiumId { get; set; }
    public string month { get; set;}
}

public class ExpenseResponseDto
{

    public int Id { get; set; }

    public string Description { get; set; } = string.Empty;
    public double TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpirationDate { get; set; }

    public int ConsortiumId { get; set; }

    public ICollection<InvoiceResponseDto> Invoices { get; set; }
}
