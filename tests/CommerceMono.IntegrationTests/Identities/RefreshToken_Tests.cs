using CommerceMono.Application.Identities.Features.Authenticating.V2;
using CommerceMono.Application.Identities.Features.RefreshingToken.V1;
using CommerceMono.Application.Users.Constants;
using CommerceMono.Modules.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommerceMono.IntegrationTests.Identities;

public class RefreshToken_Tests : AppTestBase
{
	protected override string EndpointName { get; } = "identities/refresh-token";

	public RefreshToken_Tests(TestWebApplicationFactory apiFactory) : base(apiFactory)
	{
	}

	[Fact]
	public async Task Should_RefreshToken_Success_Tests()
	{
		// Arrange
		var request = new AuthenticateRequest(UserConsts.DefaultUsername.Admin, "123qwe");
		var user = await DbContext.Users.FirstAsync(x => x.NormalizedUserName == UserConsts.DefaultUsername.Admin.ToUpper());
		var response = await Client.PostAsJsonAsync("api/v2/identities/authenticate", request);
		var authenticateResponse = await response.Content.ReadFromJsonAsync<AuthenticateResult>();

		Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authenticateResponse!.AccessToken}");

		// Act
		response = await Client.PostAsJsonAsync(Endpoint, new RefreshTokenRequest(authenticateResponse!.RefreshToken));

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var refreshTokenResponse = await response.Content.ReadFromJsonAsync<RefreshTokenResult>();
		refreshTokenResponse.Should().NotBeNull();
		refreshTokenResponse!.AccessToken.Should().NotBeNullOrEmpty();
		refreshTokenResponse!.ExpireInSeconds.Should().Be((int)TokenConsts.AccessTokenExpiration.TotalSeconds);

		var tokenKeyCaches = await CacheProvider.GetByPrefixAsync<string>($"{TokenConsts.TokenValidityKey}.{user.Id}");
		tokenKeyCaches.Should().NotBeNull();
		tokenKeyCaches!.Count().Should().Be(3);
	}

	[Fact]
	public async Task Should_RefreshToken_Fail_Test()
	{
		// Arrange
		// Act
		var response = await Client.PostAsJsonAsync(Endpoint, new { });

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
		failureResponse!.Detail.Should().Be("Refresh token cannot be empty!");
	}

	[Fact]
	public async Task Should_RefreshToken_With_Expired_Token_Test()
	{
		// Arrange
		var request = new AuthenticateRequest(UserConsts.DefaultUsername.Admin, "123qwe");
		var user = await DbContext.Users.FirstAsync(x => x.NormalizedUserName == UserConsts.DefaultUsername.Admin.ToUpper());
		var response = await Client.PostAsJsonAsync("api/v2/identities/authenticate", request);
		var authenticateResponse = await response.Content.ReadFromJsonAsync<AuthenticateResult>();

		Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authenticateResponse!.AccessToken}");

		var userTokens = await DbContext.UserTokens.Where(x => x.UserId == user.Id).ToListAsync();

		foreach (var userToken in userTokens)
		{
			userToken.ExpireDate = DateTimeOffset.Now.AddMinutes(-5);
		}

		await DbContext.SaveChangesAsync();

		await CacheProvider.FlushAsync();

		// Act
		response = await Client.PostAsJsonAsync(Endpoint, new RefreshTokenRequest(authenticateResponse!.RefreshToken));

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
		failureResponse!.Detail.Should().Be("Your session is expired!");
	}

	[Fact]
	public async Task Should_RefreshToken_With_Invalid_Token_Test()
	{
		// Arrange
		// Act
		var response = await Client.PostAsJsonAsync(Endpoint, new RefreshTokenRequest("invalid-token"));

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
	}
}
