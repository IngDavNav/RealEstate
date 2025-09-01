using Microsoft.Extensions.Options;

using RealEstate.Application.Abstractions.Files;

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RealEstate.Infrastructure.Files;

public sealed class LocalImageStorage : IImageStorage
{
    private readonly ImageStorageOptions _opt;
    public LocalImageStorage(IOptions<ImageStorageOptions> opt)
    {
        _opt = opt.Value;
    }

    public async Task<string> UploadAsync(Stream content, string fileName, string? contentType, string keyPrefix, CancellationToken cancellationToken)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_opt.AllowedExtensions.Contains(ext)) throw new InvalidOperationException($"Extensión no permitida: {ext}");

        if (content is MemoryStream ms && ms.Length > _opt.MaxBytes)
            throw new InvalidOperationException($"Archivo supera el límite de {_opt.MaxBytes} bytes");

        var safeName = $"{Guid.NewGuid():N}{ext}";
        var relative = Path.Combine(_opt.BasePath, keyPrefix.Replace("..", "").Replace('\\', '/'), safeName)
                            .Replace('\\', '/');
        var full = Path.Combine(_opt.RootPath, relative);

        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        await using var fs = new FileStream(full, FileMode.Create, FileAccess.Write, FileShare.None, 64 * 1024, useAsync: true);
        await content.CopyToAsync(fs, cancellationToken);

        return relative;
    }

    public Task DeleteAsync(string storedPath, CancellationToken cancellationToken)
    {
        try
        {
            var full = Path.Combine(_opt.RootPath, storedPath);
            if (File.Exists(full)) File.Delete(full);
        }
        catch { /* best-effort */ }
        return Task.CompletedTask;
    }
}
