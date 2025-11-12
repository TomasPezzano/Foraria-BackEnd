namespace Foraria.DTOs;

public class TransferPermissionResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int OwnerId { get; set; }
    public int TenantId { get; set; }
    public bool OwnerHasPermission { get; set; }
    public bool TenantHasPermission { get; set; }
}
