using System.Net;
using System.Net.Http.Json;
using CommerceMono.Application.Data;
using CommerceMono.Application.Roles.Constants;
using CommerceMono.Application.Roles.Features.GettingRoles.V1;
using CommerceMono.Application.Roles.Models;
using CommerceMono.IntegrationTests.Utilities;
using CommerceMono.Modules.Core.Pagination;
using FluentAssertions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceMono.IntegrationTests.Roles;

public class GetRolesTestBase : IClassFixture<TestWebApplicationFactory>
{
	protected readonly TestWebApplicationFactory _apiFactory;
	protected readonly AppDbContext _dbContext;
	protected readonly string _endpoint = "api/v1/roles";

	protected GetRolesTestBase(TestWebApplicationFactory apiFactory)
	{
		_apiFactory = apiFactory;
		var _scope = apiFactory.Services.CreateScope();
		_dbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
	}
}

public class GetRoles_Tests : GetRolesTestBase
{
	public GetRoles_Tests(TestWebApplicationFactory apiFactory) : base(apiFactory)
	{
	}

	[Fact]
	public async Task Should_Get_Roles_Test()
	{
		// Arrange
		HttpClient? client = await _apiFactory.LoginAsAdmin();
		var totalCount = await _dbContext.Roles.CountAsync();

		// Act
		var response = await client.GetAsync(_endpoint);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var roleResults = await response.Content.ReadFromJsonAsync<GetRolesResult>();

		roleResults.Should().NotBeNull();
		roleResults!.TotalCount.Should().Be(totalCount);
		roleResults!.Items!.Count().Should().Be(totalCount);
	}
}

public class GetRolesFiltered_Tests : GetRolesTestBase
{
	public GetRolesFiltered_Tests(TestWebApplicationFactory apiFactory) : base(apiFactory)
	{
	}

	[Fact]
	public async Task Should_Get_Roles_Filtered_Test()
	{
		// Arrange
		HttpClient? client = await _apiFactory.LoginAsAdmin();
		var filterText = RoleConsts.RoleName.Admin.Substring(0, 3);

		// Act
		var response = await client.GetAsync($"{_endpoint}?{nameof(PageRequest.Filters).Camelize()}={filterText}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var roleResults = await response.Content.ReadFromJsonAsync<GetRolesResult>();

		roleResults.Should().NotBeNull();
		roleResults!.TotalCount.Should().Be(1);
		roleResults!.Items!.Count().Should().Be(1);
		roleResults!.Items[0]!.Name.Should().Be(RoleConsts.RoleName.Admin);
	}
}

public class GetRolesPaginated_Tests : GetRolesTestBase
{
	public GetRolesPaginated_Tests(TestWebApplicationFactory apiFactory) : base(apiFactory)
	{
	}

	[Fact]
	public async Task Should_Get_Roles_Paginated_Test()
	{
		// Arrange
		HttpClient? client = await _apiFactory.LoginAsAdmin();
		var totalCount = await _dbContext.Roles.CountAsync();
		var sorting = nameof(Role.Name).Camelize() + " desc";
		var requestUri = $"{_endpoint}?{nameof(PageRequest.Sorting).Camelize()}={sorting}&{nameof(PageRequest.SkipCount).Camelize()}=0&{nameof(PageRequest.MaxResultCount).Camelize()}=1";

		// Act
		var response = await client.GetAsync(requestUri);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var roleResults = await response.Content.ReadFromJsonAsync<GetRolesResult>();

		roleResults.Should().NotBeNull();
		roleResults!.TotalCount.Should().Be(totalCount);
		roleResults!.Items!.Count().Should().Be(1);
		roleResults!.Items[0]!.Name.Should().NotBe(RoleConsts.RoleName.Admin);
	}
}