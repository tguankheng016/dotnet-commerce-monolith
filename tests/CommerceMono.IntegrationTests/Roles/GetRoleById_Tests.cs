using CommerceMono.Application.Roles.Constants;
using CommerceMono.Application.Roles.Features.GettingRoleById.V1;
using CommerceMono.IntegrationTests.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace CommerceMono.IntegrationTests.Roles;

public class GetRoleByIdTestBase : AppTestBase
{
	protected override string EndpointName { get; } = "role";

	protected GetRoleByIdTestBase(TestWebApplicationFactory apiFactory) : base(apiFactory)
	{
	}
}

public class GetRoleById_Tests : GetRoleByIdTestBase
{
	public GetRoleById_Tests(TestWebApplicationFactory apiFactory) : base(apiFactory)
	{
	}

	[Fact]
	public async Task Should_Get_Role_By_Id_Test()
	{
		// Arrange
		HttpClient? client = await ApiFactory.LoginAsAdmin();

		// Act	
		var response = await client.GetAsync($"{Endpoint}/1");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var roleResult = await response.Content.ReadFromJsonAsync<GetRoleByIdResult>();
		roleResult.Should().NotBeNull();
		roleResult!.Role.Should().NotBeNull();
		roleResult.Role.Id.Should().Be(1);
		roleResult.Role.Name.Should().Be(RoleConsts.RoleName.Admin);
	}
}

public class GetRoleByIdUnauthorized_Tests : GetRoleByIdTestBase
{
	public GetRoleByIdUnauthorized_Tests(TestWebApplicationFactory apiFactory) : base(apiFactory)
	{
	}

	[Fact]
	public async Task Should_Create_Role_With_Unauthorized_Error_Test()
	{
		// Arrange
		HttpClient? client = await ApiFactory.LoginAsUser();

		// Act
		var response = await client.GetAsync($"{Endpoint}/1");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
	}
}