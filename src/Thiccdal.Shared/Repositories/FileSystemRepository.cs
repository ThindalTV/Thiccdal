using System.Text.Json;

namespace Thiccdal.Shared.Repositories;
public class FileSystemRepository : IRepository
{
       private readonly string _basePath;

    public FileSystemRepository(string basePath)
    {
        _basePath = basePath;
    }

    public async Task<T?> Get<T>(string repoName, string key, CancellationToken cancellationToken, string? fileType = null)
    {
        fileType ??= "json";
        var path = $"{Path.Combine(_basePath, repoName, key)}.{fileType}";

        if (!File.Exists(path))
        {
            return default;
        }

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken);
    }

    public async Task Set<T>(string repoName, string key, T value, CancellationToken cancellationToken, string? fileType = null)
    {
        fileType ??= "json";
        var path = $"{Path.Combine(_basePath, repoName, key)}.{fileType}";

        var directory = Path.GetDirectoryName(path);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.OpenWrite(path);
        await JsonSerializer.SerializeAsync(stream, value, cancellationToken: cancellationToken);
    }

    public Task Remove(string repoName, string key, CancellationToken cancellationToken, string? fileType = null)
    {
        fileType ??= "json";
        var path = $"{Path.Combine(_basePath, repoName, key)}.{fileType}";

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }
}
