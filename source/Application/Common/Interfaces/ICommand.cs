using MediatR;

namespace Application.Common.Interfaces;

/// <summary>
/// Marker interface for commands (write operations) in CQRS pattern.
/// Commands modify state and return a result.
/// </summary>
/// <typeparam name="TResponse">The type of the response returned by the command.</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
