using Microsoft.AspNetCore.Routing;

namespace CommerceMono.Modules.Web;

public interface IMinimalEndpoint
{
    IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder);
}
