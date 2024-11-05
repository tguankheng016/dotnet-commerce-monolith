using CommerceMono.Application.Roles.Dtos;
using CommerceMono.Application.Roles.Models;
using CommerceMono.Modules.Core.CQRS;
using CommerceMono.Modules.Core.Pagination;
using CommerceMono.Modules.Core.Queryable;
using CommerceMono.Modules.Permissions;
using CommerceMono.Modules.Web;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace CommerceMono.Application.Roles.Features.GettingRoles.V1;

public class GetRolesEndpoint : IMinimalEndpoint
{
	public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		builder.MapGet($"{EndpointConfig.BaseApiPath}/identity/roles", Handle)
			.RequireAuthorization(RolePermissions.Pages_Administration_Roles)
			.WithName("GetRoles")
			.WithApiVersionSet(builder.GetApiVersionSet())
			.Produces<GetRolesResult>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.WithSummary("Get All Roles")
			.WithDescription("Get All Roles")
			.WithOpenApi()
			.HasApiVersion(1.0);

		return builder;
	}

	async Task<IResult> Handle(
		IMediator mediator, CancellationToken cancellationToken,
		[AsParameters] GetRolesRequest request
	)
	{
		var query = new GetRolesQuery()
		{
			SkipCount = request.SkipCount,
			MaxResultCount = request.MaxResultCount,
			Filters = request.Filters,
			Sorting = request.Sorting
		};

		var result = await mediator.Send(query, cancellationToken);

		return Results.Ok(result);
	}
}

// Request
public class GetRolesRequest() : PageRequest;

// Result
public class GetRolesResult : PagedResultDto<RoleDto>;

// Query
public class GetRolesQuery : PageQuery<GetRolesResult>
{
}

// Validators
public class GetRolesValidator : AbstractValidator<GetRolesQuery>
{
	public GetRolesValidator()
	{
		RuleFor(x => x.SkipCount)
			.GreaterThanOrEqualTo(0)
			.WithMessage("Page should at least greater than or equal to 0.");

		RuleFor(x => x.MaxResultCount)
			.GreaterThanOrEqualTo(0)
			.WithMessage("Page size should at least greater than or equal to 0.");
	}
}

internal class GetRolesHandler(
	RoleManager<Role> roleManager
) : IQueryHandler<GetRolesQuery, GetRolesResult>
{
	public async Task<GetRolesResult> Handle(GetRolesQuery request, CancellationToken cancellationToken)
	{
		var results = new List<RoleDto>();

		var filteredRoles = roleManager.Roles.AsNoTracking()
			.WhereIf(!string.IsNullOrWhiteSpace(request.Filters),
				e => e.Name != null && e.Name.Contains(request.Filters!)
			);

		IQueryable<Role>? pagedAndFilteredRoles = null;

		if (request.SkipCount == 0 && request.MaxResultCount == 0)
		{
			pagedAndFilteredRoles = filteredRoles
				.OrderBy(string.IsNullOrEmpty(request.Sorting) ? "id asc" : request.Sorting);
		}
		else
		{
			pagedAndFilteredRoles = filteredRoles
				.OrderBy(string.IsNullOrEmpty(request.Sorting) ? "id asc" : request.Sorting)
				.PageBy(request);
		}

		var totalCount = await filteredRoles.CountAsync(cancellationToken: cancellationToken);

		var dbList = await pagedAndFilteredRoles.ToListAsync(cancellationToken: cancellationToken);

		var mapper = new RoleMapper();

		foreach (var o in dbList)
		{
			var res = mapper.RoleToRoleDto(o);
			results.Add(res);
		}

		var pagedResults = new GetRolesResult()
		{
			TotalCount = totalCount,
			Items = results
		};

		return pagedResults;
	}
}