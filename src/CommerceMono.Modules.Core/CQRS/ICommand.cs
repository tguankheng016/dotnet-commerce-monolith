using MediatR;

namespace CommerceMono.Modules.Core.CQRS;

public interface ICommand<out T> : IRequest<T>
    where T : notnull
{
}
