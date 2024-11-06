using CommerceMono.Application.Roles.Models;
using CommerceMono.IntegrationTests.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommerceMono.IntegrationTests.Roles;

public class DeleteRoleTestBase : AppTestBase
{
	protected override string EndpointName { get; } = "role";

	protected DeleteRoleTestBase(TestWebApplicationFactory apiFactory) : base(apiFactory)
	{
	}
}

public class DeleteRole_Tests : DeleteRoleTestBase
{
	public DeleteRole_Tests(TestWebApplicationFactory apiFactory) : base(apiFactory)
	{
	}

	[Fact]
	public async Task Should_Delete_Role_Test()
	{
		// Arrange
		HttpClient? client = await ApiFactory.LoginAsAdmin();
		var newRole = new Role()
		{
			Name = "TestRole"
		};

		await DbContext.Roles.AddAsync(newRole);
		await DbContext.SaveChangesAsync();

		var totalCount = await DbContext.Roles.CountAsync();

		// Act
		var response = await client.DeleteAsync($"{Endpoint}/{newRole.Id}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var anyRoleDeletedFound = await DbContext.Roles.AnyAsync(x => x.Id == newRole.Id);
		anyRoleDeletedFound.Should().BeFalse();

		var newTotalCount = await DbContext.Roles.CountAsync();
		newTotalCount.Should().Be(totalCount - 1);
	}

	[Fact]
	public async Task Should_Delete_Static_Role_With_Error_Test()
	{
		// Arrange
		HttpClient? client = await ApiFactory.LoginAsAdmin();

		// Act
		var response = await client.DeleteAsync($"{Endpoint}/1");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
		failureResponse?.Detail.Should().Be("You cannot delete static role!");
	}

	[Fact]
	public async Task Should_Delete_Role_With_NotFound_Error_Test()
	{
		// Arrange
		HttpClient? client = await ApiFactory.LoginAsAdmin();

		// Act
		var response = await client.DeleteAsync($"{Endpoint}/100");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
	}
}

public class DeleteRoleUnauthorized_Tests : CreateRoleTestBase
{
	public DeleteRoleUnauthorized_Tests(TestWebApplicationFactory apiFactory) : base(apiFactory)
	{
	}

	[Fact]
	public async Task Should_Create_Role_With_Unauthorized_Error_Test()
	{
		// Arrange
		HttpClient? client = await ApiFactory.LoginAsUser();
		var newRole = new Role()
		{
			Name = "TestRole"
		};

		await DbContext.Roles.AddAsync(newRole);
		await DbContext.SaveChangesAsync();

		// Act
		var response = await client.DeleteAsync($"{Endpoint}/{newRole.Id}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
	}
}
