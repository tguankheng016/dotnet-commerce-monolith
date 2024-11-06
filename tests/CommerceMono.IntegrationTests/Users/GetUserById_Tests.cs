using CommerceMono.Application.Users.Constants;
using CommerceMono.Application.Users.Features.GettingUserById.V1;
using CommerceMono.IntegrationTests.Utilities;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace CommerceMono.IntegrationTests.Users;

public class GetUserByIdTestBase : AppTestBase
{
	protected override string EndpointName { get; } = "user";

	protected GetUserByIdTestBase(
		ITestOutputHelper testOutputHelper,
		TestContainers testContainers
	) : base(testOutputHelper, testContainers)
	{
	}
}

public class GetUserById_Tests : GetUserByIdTestBase
{
	public GetUserById_Tests(
		ITestOutputHelper testOutputHelper,
		TestContainers testContainers
	) : base(testOutputHelper, testContainers)
	{
	}

	[Theory]
	[InlineData(1, UserConsts.DefaultUsername.Admin)]
	[InlineData(0, null)]
	public async Task Should_Get_User_By_Id_Test(long userId, string userName)
	{
		// Arrange
		HttpClient? client = await ApiFactory.LoginAsAdmin();

		// Act	
		var response = await client.GetAsync($"{Endpoint}/{userId}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var userResult = await response.Content.ReadFromJsonAsync<GetUserByIdResult>();
		userResult.Should().NotBeNull();
		userResult!.User.Should().NotBeNull();
		userResult.User.Id.Should().Be(userId == 0 ? null : userId);
		userResult.User.UserName.Should().Be(userName);
	}

	[Fact]
	public async Task Should_Get_User_NotFound_By_Invalid_Id_Test()
	{
		// Arrange
		HttpClient? client = await ApiFactory.LoginAsAdmin();

		// Act	
		var response = await client.GetAsync($"{Endpoint}/100");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task Should_Create_User_With_Unauthorized_Error_Test()
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