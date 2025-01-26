using CommerceMono.Application.Users.Constants;
using CommerceMono.Application.Users.Features.GettingUsers.V1;
using CommerceMono.Application.Users.Models;
using CommerceMono.IntegrationTests.Utilities;
using CommerceMono.Modules.Core.Pagination;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace CommerceMono.IntegrationTests.Users;

public class GetUsersTestBase : AppTestBase
{
	protected override string EndpointName { get; } = "users";

	protected GetUsersTestBase(
		ITestOutputHelper testOutputHelper,
		TestContainers testContainers
	) : base(testOutputHelper, testContainers)
	{
	}
}

public class GetUsers_Tests : GetUsersTestBase
{
	public GetUsers_Tests(
		ITestOutputHelper testOutputHelper,
		TestContainers testContainers
	) : base(testOutputHelper, testContainers)
	{
	}

	[Fact]
	public async Task Should_Get_Users_Test()
	{
		// Arrange
		var client = await ApiFactory.LoginAsAdmin();
		var totalCount = await DbContext.Users.CountAsync();

		// Act
		var response = await client.GetAsync(Endpoint);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var userResults = await response.Content.ReadFromJsonAsync<GetUsersResult>();

		userResults.Should().NotBeNull();
		userResults!.TotalCount.Should().Be(totalCount);
		userResults!.Items!.Count().Should().Be(totalCount);
	}

	[Fact]
	public async Task Should_Get_Users_Filtered_Test()
	{
		// Arrange
		var client = await ApiFactory.LoginAsAdmin();
		var filterText = UserConsts.DefaultUsername.Admin.Substring(0, 3);

		// Act
		var response = await client.GetAsync($"{Endpoint}?{nameof(PageRequest.Filters).Camelize()}={filterText}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var userResults = await response.Content.ReadFromJsonAsync<GetUsersResult>();

		userResults.Should().NotBeNull();
		userResults!.TotalCount.Should().Be(1);
		userResults!.Items!.Count().Should().Be(1);
		userResults!.Items[0]!.UserName.Should().Be(UserConsts.DefaultUsername.Admin);
	}

	[Fact]
	public async Task Should_Get_Users_Paginated_Test()
	{
		// Arrange
		var client = await ApiFactory.LoginAsAdmin();
		var totalCount = await DbContext.Users.CountAsync();
		var sorting = nameof(User.UserName).Camelize() + " desc";
		var requestUri = $"{Endpoint}?{nameof(PageRequest.Sorting).Camelize()}={sorting}&{nameof(PageRequest.SkipCount).Camelize()}=0&{nameof(PageRequest.MaxResultCount).Camelize()}=1";

		// Act
		var response = await client.GetAsync(requestUri);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var userResults = await response.Content.ReadFromJsonAsync<GetUsersResult>();

		userResults.Should().NotBeNull();
		userResults!.TotalCount.Should().Be(totalCount);
		userResults!.Items!.Count().Should().Be(1);
		userResults!.Items[0]!.UserName.Should().NotBe(UserConsts.DefaultUsername.Admin);
	}

	[Fact]
	public async Task Should_Get_Users_Unauthorized_Error_Test()
	{
		// Arrange
		var client = await ApiFactory.LoginAsUser();
		var sorting = nameof(User.UserName).Camelize() + " desc";
		var requestUri = $"{Endpoint}?{nameof(PageRequest.Sorting).Camelize()}={sorting}&{nameof(PageRequest.SkipCount).Camelize()}=0&{nameof(PageRequest.MaxResultCount).Camelize()}=1";

		// Act
		var response = await client.GetAsync(requestUri);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
	}
}