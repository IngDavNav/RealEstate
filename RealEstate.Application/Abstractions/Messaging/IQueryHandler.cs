using System.Threading.Tasks;
using System.Threading;

namespace RealEstate.Application.Abstractions.Messaging;

public interface IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
}
