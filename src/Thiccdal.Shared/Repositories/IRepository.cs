namespace Thiccdal.Shared.Repositories;
public interface IRepository
{
    Task<T?> Get<T>(string repoName, string key, CancellationToken cancellationToken, string? fileType = null);
    Task Set<T>(string repoName, string key, T value, CancellationToken cancellationToken, string? fileType = null);
    Task Remove(string repoName, string key, CancellationToken cancellationToken, string? fileType = null);
}
