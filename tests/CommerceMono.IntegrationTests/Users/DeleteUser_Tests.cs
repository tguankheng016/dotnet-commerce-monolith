using CommerceMono.Application.Users.Models;
using CommerceMono.IntegrationTests.Utilities;
using CommerceMono.Modules.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using static Bogus.DataSets.Name;

namespace CommerceMono.IntegrationTests.Users;

public class DeleteUserTestBase : AppTestBase
{
	protected override string EndpointName { get; } = "user";

	protected DeleteUserTestBase(
		ITestOutputHelper testOutputHelper,
		TestContainers testContainers
	) : base(testOutputHelper, testContainers)
	{
	}
}


public class DeleteUser_Tests : DeleteUserTestBase
{
	public DeleteUser_Tests(
		ITestOutputHelper testOutputHelper,
		TestContainers testContainers
	) : base(testOutputHelper, testContainers)
	{
	}

	[Fact]
	public async Task Should_Delete_User_Test()
	{
		// Arrange
		var client = await ApiFactory.LoginAsAdmin();
		var newUser = GetTestUser();

		await DbContext.Users.AddAsync(newUser);
		await DbContext.SaveChangesAsync();

		var totalCount = await DbContext.Users.CountAsync();

		var newUserClient = await ApiFactory.LoginAs(newUser.UserName!);
		await newUserClient.DeleteAsync($"{Endpoint}/{newUser.Id}");

		// Act
		var response = await client.DeleteAsync($"{Endpoint}/{newUser.Id}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var anyUserDeletedFound = await DbContext.Users.AnyAsync(x => x.Id == newUser.Id);
		anyUserDeletedFound.Should().BeFalse();

		var newTotalCount = await DbContext.Users.CountAsync();
		newTotalCount.Should().Be(totalCount - 1);

		var newUserResponse = await newUserClient.DeleteAsync($"{Endpoint}/{newUser.Id}");
		newUserResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	[Fact]
	public async Task Should_Delete_Own_User_Account_With_Error_Test()
	{
		// Arrange
		var newUser = GetTestUser();

		await DbContext.Users.AddAsync(newUser);
		await DbContext.SaveChangesAsync();

		await DbContext.UserRolePermissions.AddAsync(new UserRolePermission()
		{
			Id = 0,
			UserId = newUser.Id,
			Name = UserPermissions.Pages_Administration_Users_Delete,
			IsGranted = true
		});
		await DbContext.SaveChangesAsync();

		var client = await ApiFactory.LoginAs(newUser.UserName!);

		// Act
		var response = await client.DeleteAsync($"{Endpoint}/{newUser.Id}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
		failureResponse?.Detail.Should().Be("You cannot delete your own account!");
	}

	[Fact]
	public async Task Should_Delete_Admin_User_With_Error_Test()
	{
		// Arrange
		var client = await ApiFactory.LoginAsAdmin();

		// Act
		var response = await client.DeleteAsync($"{Endpoint}/1");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
		failureResponse?.Detail.Should().Be("You cannot delete admin account!");
	}

	[Theory]
	[InlineData(0)]
	[InlineData(100)]
	public async Task Should_Delete_User_With_Invalid_UserId_Test(long userId)
	{
		// Arrange
		var client = await ApiFactory.LoginAsAdmin();

		// Act
		var response = await client.DeleteAsync($"{Endpoint}/{userId}");

		// Assert
		response.StatusCode.Should().Be(userId > 0 ? HttpStatusCode.NotFound : HttpStatusCode.BadRequest);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
	}

	[Fact]
	public async Task Should_Delete_User_With_Unauthorized_Error_Test()
	{
		// Arrange
		var client = await ApiFactory.LoginAsUser();
		var newUser = GetTestUser();

		await DbContext.Users.AddAsync(newUser);
		await DbContext.SaveChangesAsync();

		// Act
		var response = await client.DeleteAsync($"{Endpoint}/{newUser.Id}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
	}

	private User GetTestUser()
	{
		var testUser = new Faker<User>()
			.RuleFor(x => x.Id, 0)
			.RuleFor(u => u.FirstName, (f) => f.Name.FirstName(Gender.Male))
			.RuleFor(u => u.LastName, (f) => f.Name.LastName(Gender.Female))
			.RuleFor(x => x.UserName, f => f.Internet.UserName())
			.RuleFor(x => x.NormalizedUserName, (f, u) => u.UserName!.ToUpper())
			.RuleFor(x => x.Email, f => f.Internet.Email())
			.RuleFor(x => x.NormalizedEmail, (f, u) => u.Email!.ToUpper())
			.RuleFor(x => x.SecurityStamp, Guid.NewGuid().ToString());

		return testUser.Generate();
	}
}
