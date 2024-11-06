using CommerceMono.Application.Users.Dtos;
using CommerceMono.Application.Users.Models;
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

namespace CommerceMono.Application.Users.Features.GettingUsers.V1;

public class GetUsersEndpoint : IMinimalEndpoint
{
	public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		builder.MapGet($"{EndpointConfig.BaseApiPath}/users", Handle)
			.RequireAuthorization(UserPermissions.Pages_Administration_Users)
			.WithName("GetUsers")
			.WithApiVersionSet(builder.GetApiVersionSet())
			.Produces<GetUsersResult>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.WithSummary("Get All Users")
			.WithDescription("Get All Users")
			.WithOpenApi()
			.HasApiVersion(1.0);

		return builder;
	}

	async Task<IResult> Handle(
		IMediator mediator, CancellationToken cancellationToken,
		[AsParameters] GetUsersRequest request
	)
	{
		var query = new GetUsersQuery()
		{
			SkipCount = request.SkipCount ?? 0,
			MaxResultCount = request.MaxResultCount ?? 0,
			Filters = request.Filters,
			Sorting = request.Sorting
		};

		var result = await mediator.Send(query, cancellationToken);

		return Results.Ok(result);
	}
}

// Request
public class GetUsersRequest() : PageRequest;

// Result
public class GetUsersResult : PagedResultDto<UserDto>;

// Query
public class GetUsersQuery : PageQuery<GetUsersResult>;

// Validator
public class GetUsersValidator : AbstractValidator<GetUsersQuery>
{
	public GetUsersValidator()
	{
		RuleFor(x => x.SkipCount)
			.GreaterThanOrEqualTo(0)
			.WithMessage("Page should at least greater than or equal to 0.");

		RuleFor(x => x.MaxResultCount)
			.GreaterThanOrEqualTo(0)
			.WithMessage("Page size should at least greater than or equal to 0.");
	}
}

// Handler
internal class GetUsersHandler(
	UserManager<User> userManager
) : IQueryHandler<GetUsersQuery, GetUsersResult>
{
	public async Task<GetUsersResult> Handle(GetUsersQuery request, CancellationToken cancellationToken)
	{
		var results = new List<UserDto>();

		var filteredUsers = userManager.Users.AsNoTracking()
			.WhereIf(!string.IsNullOrWhiteSpace(request.Filters),
				e =>
					(e.UserName != null && e.UserName.Contains(request.Filters!)) ||
					e.FirstName.Contains(request.Filters!) ||
					e.LastName.Contains(request.Filters!) ||
					(e.Email != null && e.Email.Contains(request.Filters!))
			);

		IQueryable<User>? pagedAndFilteredUsers = null;

		if (request.SkipCount == 0 && request.MaxResultCount == 0)
		{
			pagedAndFilteredUsers = filteredUsers
				.OrderBy(string.IsNullOrEmpty(request.Sorting) ? "id asc" : request.Sorting);
		}
		else
		{
			pagedAndFilteredUsers = filteredUsers
				.OrderBy(string.IsNullOrEmpty(request.Sorting) ? "id asc" : request.Sorting)
				.PageBy(request);
		}

		var totalCount = await filteredUsers.CountAsync(cancellationToken: cancellationToken);

		var dbList = await pagedAndFilteredUsers.ToListAsync(cancellationToken: cancellationToken);

		var mapper = new UserMapper();

		foreach (var o in dbList)
		{
			var res = mapper.UserToUserDto(o);
			res.Roles = await userManager.GetRolesAsync(o);

			results.Add(res);
		}

		var pagedResults = new GetUsersResult()
		{
			TotalCount = totalCount,
			Items = results
		};

		return pagedResults;
	}
}