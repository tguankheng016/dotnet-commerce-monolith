using CommerceMono.Application.Roles.Dtos;
using CommerceMono.Application.Roles.Features.CreatingRole.V1;
using CommerceMono.IntegrationTests.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommerceMono.IntegrationTests.Roles;

public class CreateRole_Tests : AppTestBase
{
	protected override string EndpointName { get; } = "role";

	public CreateRole_Tests(TestWebApplicationFactory apiFactory) : base(apiFactory)
	{
	}

	[Fact]
	public async Task Should_Create_Role_Test()
	{
		// Arrange
		HttpClient? client = await ApiFactory.LoginAsAdmin();
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
	[InlineData("")]
	[InlineData(null)]
	public async Task Should_Create_Role_With_Invalid_Name_Test(string roleName)
	{
		// Arrange
		HttpClient? client = await ApiFactory.LoginAsAdmin();
		var request = new CreateRoleDto
		{
			Name = roleName
		};

		// Act
		var response = await client.PostAsJsonAsync(Endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
		failureResponse!.Detail.Should().Be("Please enter the name");
	}
}
