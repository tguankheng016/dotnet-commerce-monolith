using CommerceMono.Application.Roles.Constants;
using CommerceMono.Application.Roles.Features.GettingRoles.V1;
using CommerceMono.Application.Roles.Models;
using CommerceMono.IntegrationTests.Utilities;
using CommerceMono.Modules.Core.Pagination;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace CommerceMono.IntegrationTests.Roles;

public class GetRolesTestBase : AppTestBase
{
	protected override string EndpointName { get; } = "roles";

	protected GetRolesTestBase(TestWebApplicationFactory apiFactory) : base(apiFactory)
	{
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
		HttpClient? client = await ApiFactory.LoginAsAdmin();
		var totalCount = await DbContext.Roles.CountAsync();

		// Act
		var response = await client.GetAsync(Endpoint);

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
		HttpClient? client = await ApiFactory.LoginAsAdmin();
		var filterText = RoleConsts.RoleName.Admin.Substring(0, 3);

		// Act
		var response = await client.GetAsync($"{Endpoint}?{nameof(PageRequest.Filters).Camelize()}={filterText}");

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
		HttpClient? client = await ApiFactory.LoginAsAdmin();
		var totalCount = await DbContext.Roles.CountAsync();
		var sorting = nameof(Role.Name).Camelize() + " desc";
		var requestUri = $"{Endpoint}?{nameof(PageRequest.Sorting).Camelize()}={sorting}&{nameof(PageRequest.SkipCount).Camelize()}=0&{nameof(PageRequest.MaxResultCount).Camelize()}=1";

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