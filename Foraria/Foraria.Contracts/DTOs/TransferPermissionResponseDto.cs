using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foraria.Contracts.DTOs;

public class TransferPermissionResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int OwnerId { get; set; }
    public int TenantId { get; set; }
    public bool OwnerHasPermission { get; set; }
    public bool TenantHasPermission { get; set; }
}
