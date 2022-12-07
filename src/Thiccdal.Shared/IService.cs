namespace Thiccdal.Shared;

public interface IService
{
    Task Start(CancellationToken cancellationToken);
    
    Task Stop();
}
