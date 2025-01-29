using CommerceMono.Application.Roles.Models;
using CommerceMono.IntegrationTests.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace CommerceMono.IntegrationTests.Roles;

[Collection(RoleTestCollection1.Name)]
public class DeleteRoleTestBase : AppTestBase
{
	protected override string EndpointName { get; } = "role";

	protected DeleteRoleTestBase(
		ITestOutputHelper testOutputHelper,
		TestWebApplicationFactory webAppFactory
	) : base(testOutputHelper, webAppFactory)
	{
	}
}

public class DeleteRole_Tests : DeleteRoleTestBase
{
	public DeleteRole_Tests(
		ITestOutputHelper testOutputHelper,
		TestWebApplicationFactory webAppFactory
	) : base(testOutputHelper, webAppFactory)
	{
	}

	[Fact]
	public async Task Should_Delete_Role_Test()
	{
		// Arrange
		var client = await ApiFactory.LoginAsAdmin();
		var newRole = RoleFaker.GetRoleFaker().Generate();
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
		var client = await ApiFactory.LoginAsAdmin();

		// Act
		var response = await client.DeleteAsync($"{Endpoint}/1");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
		failureResponse?.Detail.Should().Be("You cannot delete static role!");
	}

	[Theory]
	[InlineData(0)]
	[InlineData(100)]
	public async Task Should_Delete_Role_With_Invalid_RoleId_Error_Test(long roleId)
	{
		// Arrange
		var client = await ApiFactory.LoginAsAdmin();

		// Act
		var response = await client.DeleteAsync($"{Endpoint}/{roleId}");

		// Assert
		response.StatusCode.Should().Be(roleId > 0 ? HttpStatusCode.NotFound : HttpStatusCode.BadRequest);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
	}

	[Fact]
	public async Task Should_Create_Role_With_Unauthorized_Error_Test()
	{
		// Arrange
		var client = await ApiFactory.LoginAsUser();
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

