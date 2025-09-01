using Microsoft.AspNetCore.Http;

namespace RealEstate.Application.Contracts.Files;

public class LocalImageUrlBuilder : IImageUrlBuilder
{
    public string ToPublicUrl(string storedPath, HttpRequest req)
    {
        var rel = storedPath.StartsWith("/") ? storedPath : "/" + storedPath;
        return $"{req.Scheme}://{req.Host}{req.PathBase}{rel.Replace("\\", "/")}";
    }
}
