namespace Thiccdal.Interfaces;

public interface IService
{
    Task Start(CancellationToken cancellationToken);
    
    Task Stop();
}
