using CommerceMono.Application.Roles.Constants;
using CommerceMono.Application.Roles.Features.GettingRoleById.V1;
using CommerceMono.IntegrationTests.Utilities;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace CommerceMono.IntegrationTests.Roles;

public class GetRoleByIdTestBase : AppTestBase
{
	protected override string EndpointName { get; } = "role";

	protected GetRoleByIdTestBase(
		ITestOutputHelper testOutputHelper,
		TestContainers testContainers
	) : base(testOutputHelper, testContainers)
	{
	}
}

public class GetRoleById_Tests : GetRoleByIdTestBase
{
	public GetRoleById_Tests(
		ITestOutputHelper testOutputHelper,
		TestContainers testContainers
	) : base(testOutputHelper, testContainers)
	{
	}

	[Theory]
	[InlineData(1, RoleConsts.RoleName.Admin)]
	[InlineData(0, "")]
	public async Task Should_Get_Role_By_Id_Test(long roleId, string roleName)
	{
		// Arrange
		var client = await ApiFactory.LoginAsAdmin();

		// Act	
		var response = await client.GetAsync($"{Endpoint}/{roleId}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var roleResult = await response.Content.ReadFromJsonAsync<GetRoleByIdResult>();
		roleResult.Should().NotBeNull();
		roleResult!.Role.Should().NotBeNull();
		roleResult.Role.Id.Should().Be(roleId == 0 ? null : roleId);
		roleResult.Role.Name.Should().Be(roleName);
	}

	[Theory]
	[InlineData(100)]
	[InlineData(-3)]
	public async Task Should_Get_Role_Error_By_Invalid_RoleId_Test(long roleId)
	{
		// Arrange
		var client = await ApiFactory.LoginAsAdmin();

		// Act	
		var response = await client.GetAsync($"{Endpoint}/{roleId}");

		// Assert
		response.StatusCode.Should().Be(roleId > 0 ? HttpStatusCode.NotFound : HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task Should_Create_Role_With_Unauthorized_Error_Test()
	{
		// Arrange
		var client = await ApiFactory.LoginAsUser();

		// Act
		var response = await client.GetAsync($"{Endpoint}/1");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
	}
}