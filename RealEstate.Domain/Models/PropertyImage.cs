namespace RealEstate.Domain.Models;

public class PropertyImage
{
    public int IdPropertyImage { get; init; }
    public int IdProperty { get; set; }
    public string File { get; set; } = null!;
    public bool Enabled { get; set; } = true;
}
