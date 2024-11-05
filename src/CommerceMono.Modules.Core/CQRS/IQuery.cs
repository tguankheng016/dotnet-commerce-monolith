using MediatR;

namespace CommerceMono.Modules.Core.CQRS;

public interface IQuery<out T> : IRequest<T>
    where T : notnull
{
}