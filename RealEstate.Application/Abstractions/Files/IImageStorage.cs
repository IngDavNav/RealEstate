using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RealEstate.Application.Abstractions.Files;

public interface IImageStorage
{
    Task<string> UploadAsync(Stream content, string fileName, string? contentType, string keyPrefix, CancellationToken cancellationToken);
    Task DeleteAsync(string storedPath, CancellationToken cancellationToken);
}
