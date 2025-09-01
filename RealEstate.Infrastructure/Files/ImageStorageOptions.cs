namespace RealEstate.Infrastructure.Files;

public class ImageStorageOptions
{
    public string RootPath { get; set; } = "wwwroot";
    public string BasePath { get; set; } = "uploads";
    public long MaxBytes { get; set; } = 5 * 1024 * 1024; // 5MB
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp"];
}
