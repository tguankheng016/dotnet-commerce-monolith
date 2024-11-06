using CommerceMono.Application.Identities.Features.Authenticating.V1;
using CommerceMono.Application.Users.Constants;
using CommerceMono.Modules.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace CommerceMono.IntegrationTests.Identities;

public class Authenticate_Tests : AppTestBase
{
	protected override string EndpointVersion { get; } = "v2";
	protected override string EndpointName { get; } = "identities/authenticate";

	public Authenticate_Tests(
		ITestOutputHelper testOutputHelper,
		TestContainers testContainers
	) : base(testOutputHelper, testContainers)
	{
	}

	[Theory]
	[InlineData(UserConsts.DefaultUsername.Admin, "123qwe")]
	[InlineData(UserConsts.DefaultUsername.User, "123qwe")]
	public async Task Should_Authenticate_As_Default_User_Test(string username, string password)
	{
		// Arrange
		var request = new AuthenticateRequest(username, password);
		var user = await DbContext.Users.FirstAsync(x => x.NormalizedUserName == username.ToUpper());

		// Act
		var response = await Client.PostAsJsonAsync(Endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var authenticateResponse = await response.Content.ReadFromJsonAsync<AuthenticateResult>();
		authenticateResponse.Should().NotBeNull();
		authenticateResponse!.AccessToken.Should().NotBeNullOrEmpty();
		authenticateResponse!.ExpireInSeconds.Should().Be((int)TokenConsts.AccessTokenExpiration.TotalSeconds);
		authenticateResponse!.RefreshToken.Should().NotBeNullOrEmpty();
		authenticateResponse!.RefreshTokenExpireInSeconds.Should().Be((int)TokenConsts.RefreshTokenExpiration.TotalSeconds);

		var tokenKeyCaches = await CacheProvider.GetByPrefixAsync<string>($"{TokenConsts.TokenValidityKey}.{user.Id}");
		tokenKeyCaches.Should().NotBeNull();
		// Access and Refresh
		tokenKeyCaches!.Count().Should().Be(2);

		var securityStampCaches = await CacheProvider.GetByPrefixAsync<string>($"{TokenConsts.SecurityStampKey}.{user.Id}");
		securityStampCaches.Should().NotBeNull();
		securityStampCaches!.Count().Should().Be(1);
	}

	[Theory]
	[InlineData(UserConsts.DefaultUsername.Admin, "123123")]
	[InlineData(UserConsts.DefaultUsername.User, "123123")]
	[InlineData(null, null, "Please enter the username or email address")]
	[InlineData(UserConsts.DefaultUsername.Admin, null, "Please enter the password")]
	public async Task Should_Not_Authenticate_With_Invalid_Credentials_Test(string username, string password, string errorMessage = "Invalid username or password!")
	{
		// Arrange
		var request = new AuthenticateRequest(username, password);

		// Act
		var response = await Client.PostAsJsonAsync(Endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
		failureResponse!.Detail.Should().Be(errorMessage);
	}
}
