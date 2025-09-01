namespace RealEstate.Application.Contracts.Files;

public class ImageUpload
{
    public required string FileName { get; init; }
    public required byte[] Content { get; init; }
    public string? ContentType { get; init; }
}
