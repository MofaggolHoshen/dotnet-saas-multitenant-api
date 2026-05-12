using MediatR;

namespace Application.Common.Interfaces;

/// <summary>
/// Marker interface for queries (read operations) in CQRS pattern.
/// Queries retrieve data without modifying state.
/// </summary>
/// <typeparam name="TResponse">The type of the response returned by the query.</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
