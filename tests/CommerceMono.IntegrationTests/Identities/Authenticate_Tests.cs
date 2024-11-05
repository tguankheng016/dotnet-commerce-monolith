using System.Net;
using System.Net.Http.Json;
using CommerceMono.Application.Data;
using CommerceMono.Application.Identities.Features.Authenticating.V1;
using CommerceMono.Application.Users.Constants;
using CommerceMono.Modules.Caching;
using CommerceMono.Modules.Security;
using EasyCaching.Core;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceMono.IntegrationTests.Identities;

public class Authenticate_Tests : IClassFixture<TestWebApplicationFactory>
{
	private readonly HttpClient _client;
	private readonly AppDbContext _dbContext;
	private readonly IEasyCachingProvider _cacheProvider;
	private readonly string _endpoint = "api/v2/identities/authenticate";

	public Authenticate_Tests(TestWebApplicationFactory apiFactory)
	{
		_client = apiFactory.CreateClient();
		var _scope = apiFactory.Services.CreateScope();
		_dbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
		_cacheProvider = _scope.ServiceProvider.GetRequiredService<ICacheManager>().GetCachingProvider();
	}

	[Theory]
	[InlineData(UserConsts.DefaultUsername.Admin, "123qwe")]
	[InlineData(UserConsts.DefaultUsername.User, "123qwe")]
	public async Task Should_Authenticate_As_Default_User_Test(string username, string password)
	{
		// Arrange
		var request = new AuthenticateRequest(username, password);
		var user = await _dbContext.Users.FirstAsync(x => x.NormalizedUserName == username.ToUpper());

		// Act
		var response = await _client.PostAsJsonAsync(_endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var authenticateResponse = await response.Content.ReadFromJsonAsync<AuthenticateResult>();
		authenticateResponse.Should().NotBeNull();
		authenticateResponse!.AccessToken.Should().NotBeNullOrEmpty();
		authenticateResponse!.ExpireInSeconds.Should().Be((int)TokenConsts.AccessTokenExpiration.TotalSeconds);
		authenticateResponse!.RefreshToken.Should().NotBeNullOrEmpty();
		authenticateResponse!.RefreshTokenExpireInSeconds.Should().Be((int)TokenConsts.RefreshTokenExpiration.TotalSeconds);

		var tokenKeyCaches = await _cacheProvider.GetByPrefixAsync<string>($"{TokenConsts.TokenValidityKey}.{user.Id}");
		tokenKeyCaches.Should().NotBeNull();
		// Access and Refresh
		tokenKeyCaches!.Count().Should().Be(2);

		var securityStampCaches = await _cacheProvider.GetByPrefixAsync<string>($"{TokenConsts.SecurityStampKey}.{user.Id}");
		securityStampCaches.Should().NotBeNull();
		securityStampCaches!.Count().Should().Be(1);
	}

	[Theory]
	[InlineData(UserConsts.DefaultUsername.Admin, "123123")]
	[InlineData(UserConsts.DefaultUsername.User, "123123")]
	[InlineData(null, null)]
	public async Task Should_Not_Authenticate_With_Invalid_Credentials_Test(string username, string password)
	{
		// Arrange
		var request = new AuthenticateRequest(username, password);

		// Act
		var response = await _client.PostAsJsonAsync(_endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}
}
