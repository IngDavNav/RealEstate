namespace RealEstate.Application.Contracts.Properties;

public class PropertyImageDto
{
    public int IdPropertyImage { get; set; }
    public int IdProperty { get; set; }
    public string File { get; set; } = null!;
    public bool Enabled { get; set; }
    public string Url { get; set; }
}