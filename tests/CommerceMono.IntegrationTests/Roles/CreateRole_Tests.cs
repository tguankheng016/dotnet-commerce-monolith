using CommerceMono.Application.Roles.Dtos;
using CommerceMono.Application.Roles.Features.CreatingRole.V1;
using CommerceMono.IntegrationTests.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace CommerceMono.IntegrationTests.Roles;

[Collection(RoleTestCollection1.Name)]
public class CreateRoleTestBase : AppTestBase
{
	protected override string EndpointName { get; } = "role";

	protected CreateRoleTestBase(
		ITestOutputHelper testOutputHelper,
		TestWebApplicationFactory webAppFactory
	) : base(testOutputHelper, webAppFactory)
	{
	}
}

public class CreateRole_Tests : CreateRoleTestBase
{
	public CreateRole_Tests(
		ITestOutputHelper testOutputHelper,
		TestWebApplicationFactory webAppFactory
	) : base(testOutputHelper, webAppFactory)
	{
	}

	[Fact]
	public async Task Should_Create_Role_Test()
	{
		// Arrange
		var client = await ApiFactory.LoginAsAdmin();
		var totalCount = await DbContext.Roles.CountAsync();
		var request = new CreateRoleDto
		{
			Name = "TestRole"
		};

		// Act
		var response = await client.PostAsJsonAsync(Endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var createResult = await response.Content.ReadFromJsonAsync<CreateRoleResult>();
		createResult.Should().NotBeNull();
		createResult!.Role.Should().NotBeNull();
		createResult!.Role.Id.Should().BeGreaterThan(0);

		var newTotalCount = await DbContext.Roles.CountAsync();
		newTotalCount.Should().Be(totalCount + 1);
	}

	[Theory]
	[ClassData(typeof(CreateRoleErrorByInvalidInputTestData))]
	public async Task Should_Create_Role_With_Invalid_Input_Test(long? roleId, string? roleName, string errorMessage)
	{
		// Arrange
		var client = await ApiFactory.LoginAsAdmin();
		var request = new CreateRoleDto
		{
			Id = roleId,
			Name = roleName!
		};

		// Act
		var response = await client.PostAsJsonAsync(Endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
		failureResponse!.Detail.Should().Be(errorMessage);
	}

	[Fact]
	public async Task Should_Create_Role_With_Unauthorized_Error_Test()
	{
		// Arrange
		var client = await ApiFactory.LoginAsUser();
		var request = new CreateRoleDto
		{
			Name = "TestRole"
		};

		// Act
		var response = await client.PostAsJsonAsync(Endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
	}

	public class CreateRoleErrorByInvalidInputTestData : TheoryData<long?, string?, string>
	{
		public CreateRoleErrorByInvalidInputTestData()
		{
			Add(null, "", "Please enter the name");
			Add(null, null, "Please enter the name");
			Add(1, "TestRole", "Invalid role id");
		}
	}
}
