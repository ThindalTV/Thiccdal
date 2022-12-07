using MediatR;

namespace Thiccdal.Shared.Contracts;

public class TestRequest : INotification
{
    public string Message { get; init; } = null!;
}
