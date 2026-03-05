// ShareLinkResponse.cs — Public paylaşım link sonucu DTO'su.

namespace ImageForge.Application.DTOs.Gallery;

public class ShareLinkResponse
{
    public Guid ImageId { get; set; }
    public bool IsPublic { get; set; }
    public string? ShareToken { get; set; }
}
