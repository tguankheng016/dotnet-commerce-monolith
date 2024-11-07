using CommerceMono.Application.Users.Features.GettingUserPermissions.V1;
using CommerceMono.Application.Users.Models;
using CommerceMono.IntegrationTests.Utilities;
using CommerceMono.Modules.Permissions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace CommerceMono.IntegrationTests.Users;

public class UserPermissionsTestBase : AppTestBase
{
	protected override string EndpointName { get; } = "users";

	protected UserPermissionsTestBase(
		ITestOutputHelper testOutputHelper,
		TestContainers testContainers
	) : base(testOutputHelper, testContainers)
	{
	}
}

public class UserPermissions_Tests : UserPermissionsTestBase
{
	public UserPermissions_Tests(
		ITestOutputHelper testOutputHelper,
		TestContainers testContainers
	) : base(testOutputHelper, testContainers)
	{
	}

	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	public async Task Should_Get_Correct_User_Permissions_Test(long userId)
	{
		var user = await DbContext.Users.FirstAsync(x => x.Id == userId);
		var expectedGetPermissions = GetExpectedGetPermissions(userId);
		var updatedPermissions = GetUpdatedPermissions(userId);
		var expectedGrantedPermissions = GetExpectedrantedPermissions(userId);
		var expectedProhibitedPermissions = GetExpectedProhibitedPermissions(userId);

		var client = await ApiFactory.LoginAsAdmin();

		await GetUserPermissions_Test(client, user, expectedGetPermissions);
		await UpdateUserPermissions_Test(client, user, updatedPermissions, expectedGrantedPermissions, expectedProhibitedPermissions);
		await ResetUserPermissions_Test(client, user);
	}

	private async Task GetUserPermissions_Test(HttpClient client, User user, List<string> expectedGetPermissions)
	{
		// Arrange
		// Act
		var response = await client.GetAsync($"{EndpointPrefix}/{EndpointVersion}/user/{user.Id}/permissions");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var userPermissions = await response.Content.ReadFromJsonAsync<GetUserPermissionsResult>();
		userPermissions.Should().NotBeNull();
		userPermissions!.Items.Should().BeEquivalentTo(expectedGetPermissions);
	}

	private async Task UpdateUserPermissions_Test(HttpClient client, User user, List<string> updatedPermissions, List<string> expectedGrantedPermissions, List<string> expectedProhibitedPermissions)
	{
		// Arrange
		// Act
		var response = await client.PutAsJsonAsync($"{EndpointPrefix}/{EndpointVersion}/user/{user.Id}/permissions", updatedPermissions);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var grantedPemissions = await DbContext.UserRolePermissions
			.Where(x => x.UserId == user.Id && x.IsGranted)
			.Select(x => x.Name).ToListAsync();

		grantedPemissions.Should().BeEquivalentTo(expectedGrantedPermissions);

		var prohibitedPermissions = await DbContext.UserRolePermissions
			.Where(x => x.UserId == user.Id && !x.IsGranted)
			.Select(x => x.Name).ToListAsync();

		prohibitedPermissions.Should().BeEquivalentTo(expectedProhibitedPermissions);
	}

	private async Task ResetUserPermissions_Test(HttpClient client, User user)
	{
		// Arrange
		// Act
		var response = await client.PutAsync($"{EndpointPrefix}/{EndpointVersion}/user/{user.Id}/reset-permissions", null);

		// Assert
		var userPemissions = await DbContext.UserRolePermissions
			.Where(x => x.UserId == user.Id)
			.Select(x => x.Name).ToListAsync();

		userPemissions.Should().BeEmpty();
	}

	private List<string> GetExpectedGetPermissions(long userId)
	{
		if (userId == 1)
		{
			// Admin
			return AppPermissionProvider.GetPermissions().Select(x => x.Name).ToList();
		}
		else
		{
			return new List<string>();
		}
	}

	private List<string> GetUpdatedPermissions(long userId)
	{
		if (userId == 1)
		{
			// Admin
			var allPermissions = AppPermissionProvider.GetPermissions().Select(x => x.Name).ToList();
			return allPermissions.Where(x => x != RolePermissions.Pages_Administration_Roles_Delete).ToList();
		}
		else
		{
			return new List<string>()
			{
				UserPermissions.Pages_Administration_Users,
				RolePermissions.Pages_Administration_Roles
			};
		}
	}

	private List<string> GetExpectedrantedPermissions(long userId)
	{
		if (userId == 1)
		{
			// Admin
			return new List<string>();
		}
		else
		{
			return GetUpdatedPermissions(userId);
		}
	}

	private List<string> GetExpectedProhibitedPermissions(long userId)
	{
		if (userId == 1)
		{
			// Admin
			return new List<string>()
			{
				RolePermissions.Pages_Administration_Roles_Delete
			};
		}
		else
		{
			return new List<string>();
		}
	}
}
