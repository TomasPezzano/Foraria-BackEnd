using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foraria.Contracts.DTOs;

public class SupplierResponseDto
{
    public int Id { get; set; }

    public string CommercialName { get; set; }


    public string BusinessName { get; set; }


    public string Cuit { get; set; } = string.Empty;


    public string SupplierCategory { get; set; }



    public string? Email { get; set; }


    public string? Phone { get; set; }


    public string? Address { get; set; }


    public string? ContactPerson { get; set; }


    public string? Observations { get; set; }

    public bool Active { get; set; }

    public DateTime RegistrationDate { get; set; }

}
