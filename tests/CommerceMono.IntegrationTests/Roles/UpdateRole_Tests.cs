using System.Collections;
using CommerceMono.Application.Roles.Constants;
using CommerceMono.Application.Roles.Dtos;
using CommerceMono.Application.Roles.Features.UpdatingRole.V1;
using CommerceMono.IntegrationTests.Utilities;
using CommerceMono.Modules.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace CommerceMono.IntegrationTests.Roles;

public class UpdateRoleTestBase : AppTestBase
{
	protected override string EndpointName { get; } = "role";

	protected UpdateRoleTestBase(
		ITestOutputHelper testOutputHelper,
		TestContainers testContainers
	) : base(testOutputHelper, testContainers)
	{
	}
}

public class UpdateRole_Tests : UpdateRoleTestBase
{
	public UpdateRole_Tests(
		ITestOutputHelper testOutputHelper,
		TestContainers testContainers
	) : base(testOutputHelper, testContainers)
	{
	}

	[Fact]
	public async Task Should_Update_Role_Test()
	{
		// Arrange
		HttpClient? client = await ApiFactory.LoginAsAdmin();
		var totalCount = await DbContext.Roles.CountAsync();
		var request = new EditRoleDto
		{
			Id = 2,
			Name = RoleConsts.RoleName.User,
			IsDefault = false
		};

		// Act
		var response = await client.PutAsJsonAsync(Endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var updateResult = await response.Content.ReadFromJsonAsync<UpdateRoleResult>();
		updateResult.Should().NotBeNull();
		updateResult!.Role.Should().NotBeNull();
		updateResult!.Role.IsDefault.Should().BeFalse();
		updateResult!.Role.Id.Should().Be(2);

		var newTotalCount = await DbContext.Roles.CountAsync();
		newTotalCount.Should().Be(totalCount);
	}

	[Fact]
	public async Task Should_Get_Update_Role_NotFound_By_Invalid_Id_Test()
	{
		// Arrange
		HttpClient? client = await ApiFactory.LoginAsAdmin();
		var request = new EditRoleDto
		{
			Id = 100,
			Name = RoleConsts.RoleName.User,
			IsDefault = false
		};

		// Act	
		var response = await client.PutAsJsonAsync(Endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task Should_Get_Update_Static_Role_Error_Test()
	{
		// Arrange
		HttpClient? client = await ApiFactory.LoginAsAdmin();
		var request = new EditRoleDto
		{
			Id = 1,
			Name = "TestRole"
		};

		// Act	
		var response = await client.PutAsJsonAsync(Endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
		failureResponse!.Detail.Should().Be("You cannot change the name of static role");
	}

	[Fact]
	public async Task Should_Update_Role_With_Unauthorized_Error_Test()
	{
		// Arrange
		HttpClient? client = await ApiFactory.LoginAsUser();
		var request = new EditRoleDto
		{
			Id = 2,
			Name = "TestRole"
		};

		// Act
		var response = await client.PostAsJsonAsync(Endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
	}
}

public class UpdateRolePermissions_Tests : UpdateRoleTestBase
{
	public UpdateRolePermissions_Tests(
		ITestOutputHelper testOutputHelper,
		TestContainers testContainers
	) : base(testOutputHelper, testContainers)
	{
	}

	[Theory]
	[ClassData(typeof(GetUpdatedRolePermissionsTestData))]
	public async Task Should_Update_Role_Permissions_Test(long roleId, List<string> permissionsToUpdate, List<string> expectedGrantedPermissions, List<string> expectedProhibitedPermissions)
	{
		// Arrange
		HttpClient? client = await ApiFactory.LoginAsAdmin();
		var role = await DbContext.Roles.FirstAsync(x => x.Id == roleId);
		var request = new EditRoleDto
		{
			Id = roleId,
			Name = role.Name!,
			GrantedPermissions = permissionsToUpdate
		};

		// Act
		var response = await client.PutAsJsonAsync(Endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var expectedPermissions = await DbContext.UserRolePermissions
			.Where(x => x.RoleId == roleId && x.IsGranted).Select(x => x.Name).ToListAsync();
		expectedPermissions.Should().BeEquivalentTo(expectedGrantedPermissions);

		var prohibitedPermissions = await DbContext.UserRolePermissions
			.Where(x => x.RoleId == roleId && !x.IsGranted).Select(x => x.Name).ToListAsync();
		prohibitedPermissions.Should().BeEquivalentTo(expectedProhibitedPermissions);
	}

	private static List<string> GetPermissionsToUpdate(int scenario = 0)
	{
		switch (scenario)
		{
			case 0:
				{
					// Admin With Prohibited
					var allPermissions = AppPermissionProvider.GetPermissions().Select(x => x.Name).ToList();
					var permissionsToUpdate = allPermissions.Where(x => x != UserPermissions.Pages_Administration_Users).ToList();

					return permissionsToUpdate;
				}
			case 1:
				{
					// User Without Prohibited
					return new List<string>()
					{
						UserPermissions.Pages_Administration_Users,
						UserPermissions.Pages_Administration_Users_Create
					};
				}
		}

		return new List<string>();
	}

	private static List<string> GetExpectedPermissions(int scenario = 0)
	{
		switch (scenario)
		{
			case 0:
				{
					// Admin With Prohibited
					// return empty because admin is granted by default
					return new List<string>();
				}
			case 1:
				{
					// User Without Prohibited
					return new List<string>()
					{
						UserPermissions.Pages_Administration_Users,
						UserPermissions.Pages_Administration_Users_Create
					};
				}
		}

		return new List<string>();
	}

	private class GetUpdatedRolePermissionsTestData : IEnumerable<object[]>
	{
		public IEnumerator<object[]> GetEnumerator()
		{
			// Admin With Prohibited
			yield return new object[]
			{
				1,
				GetPermissionsToUpdate(0),
				GetExpectedPermissions(0),
				new List<string>()
				{
					UserPermissions.Pages_Administration_Users
				}
			};
			// User Without Prohibited
			yield return new object[]
			{
				2,
				GetPermissionsToUpdate(1),
				GetExpectedPermissions(1),
				new List<string>()
			};
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
