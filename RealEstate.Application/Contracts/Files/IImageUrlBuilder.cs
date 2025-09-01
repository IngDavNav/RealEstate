using Microsoft.AspNetCore.Http;

namespace RealEstate.Application.Contracts.Files;

public interface IImageUrlBuilder
{
    string ToPublicUrl(string storedPath, HttpRequest req);

}
