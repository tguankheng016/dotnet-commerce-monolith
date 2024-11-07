using CommerceMono.Application.Users.Services;
using CommerceMono.Modules.Core.CQRS;
using CommerceMono.Modules.Permissions;
using CommerceMono.Modules.Web;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CommerceMono.Application.Users.Features.GettingUserPermissions.V1;

public class GetUserPermissionsEndpoint : IMinimalEndpoint
{
	public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		builder.MapGet($"{EndpointConfig.BaseApiPath}/identity/user/permissions/{{userid:long}}", Handle)
			.RequireAuthorization(UserPermissions.Pages_Administration_Users_ChangePermissions)
			.WithName("GetUserPermissions")
			.WithApiVersionSet(builder.GetApiVersionSet())
			.Produces<GetUserPermissionsResult>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.WithSummary("Get User Permissions")
			.WithDescription("Get User Permissions")
			.WithOpenApi()
			.HasApiVersion(1.0);

		return builder;
	}

	async Task<IResult> Handle(
		IMediator mediator, CancellationToken cancellationToken,
		[FromRoute] long UserId
	)
	{
		var query = new GetUserPermissionsQuery(UserId);

		var result = await mediator.Send(query, cancellationToken);

		return Results.Ok(result);
	}
}

// Result
public record GetUserPermissionsResult(IList<string> Items);

// Query
public record GetUserPermissionsQuery(long Id) : IQuery<GetUserPermissionsResult>;

// Validator
public class GetUserPermissionsQueryValidator : AbstractValidator<GetUserPermissionsQuery>
{
	public GetUserPermissionsQueryValidator()
	{
		RuleFor(x => x.Id).GreaterThan(0).WithMessage("Invalid user id");
	}
}

// Handler
internal class GetUserPermissionsHandler(
	IUserRolePermissionManager userRolePermissionManager
) : IQueryHandler<GetUserPermissionsQuery, GetUserPermissionsResult>
{
	public async Task<GetUserPermissionsResult> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
	{
		var results = await userRolePermissionManager
			.SetUserPermissionAsync(request.Id, cancellationToken);

		return new GetUserPermissionsResult(results.Select(x => x.Key).ToList());
	}
}