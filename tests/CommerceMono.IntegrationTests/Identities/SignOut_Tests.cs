using System.Net;
using System.Net.Http.Json;
using CommerceMono.Application.Data;
using CommerceMono.Application.Identities.Features.Authenticating.V2;
using CommerceMono.Application.Users.Constants;
using CommerceMono.Modules.Caching;
using CommerceMono.Modules.Security;
using EasyCaching.Core;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceMono.IntegrationTests.Identities;

public class SignOut_Tests : IClassFixture<TestWebApplicationFactory>
{
	private readonly HttpClient _client;
	private readonly AppDbContext _dbContext;
	private readonly IEasyCachingProvider _cacheProvider;
	private readonly string _endpoint = "api/v1/identities/sign-out";

	public SignOut_Tests(TestWebApplicationFactory apiFactory)
	{
		_client = apiFactory.CreateClient();
		var _scope = apiFactory.Services.CreateScope();
		_dbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
		_cacheProvider = _scope.ServiceProvider.GetRequiredService<ICacheManager>().GetCachingProvider();
	}

	[Fact]
	public async Task Should_SignOut_Success__Tests()
	{
		// Arrange
		var request = new AuthenticateRequest(UserConsts.DefaultUsername.Admin, "123qwe");
		var user = await _dbContext.Users.FirstAsync(x => x.NormalizedUserName == UserConsts.DefaultUsername.Admin.ToUpper());
		var response = await _client.PostAsJsonAsync("api/v2/identities/authenticate", request);
		var authenticateResponse = await response.Content.ReadFromJsonAsync<AuthenticateResult>();

		_client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authenticateResponse!.AccessToken}");

		// Act
		response = await _client.PostAsync(_endpoint, null);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var tokenKeyCaches = await _cacheProvider.GetByPrefixAsync<string>($"{TokenConsts.TokenValidityKey}.{user.Id}");
		tokenKeyCaches.Should().NotBeNull();
		tokenKeyCaches!.Count().Should().Be(0);
	}
}
