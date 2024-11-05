using System.Net;
using System.Net.Http.Json;
using CommerceMono.Application.Data;
using CommerceMono.Application.Identities.Features.Authenticating.V2;
using CommerceMono.Application.Identities.Features.RefreshingToken.V1;
using CommerceMono.Application.Users.Constants;
using CommerceMono.Modules.Caching;
using CommerceMono.Modules.Security;
using EasyCaching.Core;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceMono.IntegrationTests.Identities;

public class RefreshToken_Tests : IClassFixture<TestWebApplicationFactory>
{
	private readonly HttpClient _client;
	private readonly AppDbContext _dbContext;
	private readonly IEasyCachingProvider _cacheProvider;
	private readonly string _endpoint = "api/v1/identities/refresh-token";

	public RefreshToken_Tests(TestWebApplicationFactory apiFactory)
	{
		_client = apiFactory.CreateClient();
		var _scope = apiFactory.Services.CreateScope();
		_dbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
		_cacheProvider = _scope.ServiceProvider.GetRequiredService<ICacheManager>().GetCachingProvider();
	}

	[Fact]
	public async Task Should_RefreshToken_Success_Tests()
	{
		// Arrange
		var request = new AuthenticateRequest(UserConsts.DefaultUsername.Admin, "123qwe");
		var user = await _dbContext.Users.FirstAsync(x => x.NormalizedUserName == UserConsts.DefaultUsername.Admin.ToUpper());
		var response = await _client.PostAsJsonAsync("api/v2/identities/authenticate", request);
		var authenticateResponse = await response.Content.ReadFromJsonAsync<AuthenticateResult>();

		_client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authenticateResponse!.AccessToken}");

		// Act
		response = await _client.PostAsJsonAsync(_endpoint, new RefreshTokenRequest(authenticateResponse!.RefreshToken));

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var refreshTokenResponse = await response.Content.ReadFromJsonAsync<RefreshTokenResult>();
		refreshTokenResponse.Should().NotBeNull();
		refreshTokenResponse!.AccessToken.Should().NotBeNullOrEmpty();
		refreshTokenResponse!.ExpireInSeconds.Should().Be((int)TokenConsts.AccessTokenExpiration.TotalSeconds);

		var tokenKeyCaches = await _cacheProvider.GetByPrefixAsync<string>($"{TokenConsts.TokenValidityKey}.{user.Id}");
		tokenKeyCaches.Should().NotBeNull();
		tokenKeyCaches!.Count().Should().Be(3);
	}

	[Fact]
	public async Task Should_RefreshToken_Fail_Test()
	{
		// Arrange
		// Act
		var response = await _client.PostAsJsonAsync(_endpoint, new { });

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
		var user = await _dbContext.Users.FirstAsync(x => x.NormalizedUserName == UserConsts.DefaultUsername.Admin.ToUpper());
		var response = await _client.PostAsJsonAsync("api/v2/identities/authenticate", request);
		var authenticateResponse = await response.Content.ReadFromJsonAsync<AuthenticateResult>();

		_client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authenticateResponse!.AccessToken}");

		var userTokens = await _dbContext.UserTokens.Where(x => x.UserId == user.Id).ToListAsync();

		foreach (var userToken in userTokens)
		{
			userToken.ExpireDate = DateTimeOffset.Now.AddMinutes(-5);
		}

		await _dbContext.SaveChangesAsync();

		await _cacheProvider.FlushAsync();

		// Act
		response = await _client.PostAsJsonAsync(_endpoint, new RefreshTokenRequest(authenticateResponse!.RefreshToken));

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
		var response = await _client.PostAsJsonAsync(_endpoint, new RefreshTokenRequest("invalid-token"));

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
	}
}
