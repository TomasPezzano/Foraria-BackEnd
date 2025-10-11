using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Foraria.Contracts.DTOs;

public class SupplierRequestDto
{
    [Required]
    [MaxLength(200)]
    public string CommercialName { get; set; }

    [MaxLength(200)]
    public string BusinessName { get; set; }

    [Required]
    public string Cuit { get; set; } = string.Empty;

    [Required]
    public string supplierCategory { get; set; }


    [EmailAddress(ErrorMessage = "Debe ser email")]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(200)]
    public string? ContactPerson { get; set; }

    [MaxLength(1000)]
    public string? Observations { get; set; }

   
}
