
using System.Net;
using System.Net.Http.Json;
using CommerceMono.Application.Data;
using CommerceMono.Application.Identities.Features.Authenticating.V1;
using CommerceMono.Application.Users.Constants;
using CommerceMono.Modules.Security;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceMono.IntegrationTests.Identities;

public class Authenticate_Tests : IClassFixture<TestWebApplicationFactory>
{
	private readonly HttpClient _client;
	private readonly AppDbContext _dbContext;
	private readonly string _endpoint = "api/identities/authenticate";

	public Authenticate_Tests(TestWebApplicationFactory apiFactory)
	{
		_client = apiFactory.CreateClient();
		var _scope = apiFactory.Services.CreateScope();
		_dbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
	}

	[Fact]
	public async Task Should_Authenticate_As_Default_Admin_Test()
	{
		// Arrange
		var request = new AuthenticateRequest(UserConsts.DefaultUsername.Admin, "123qwe");

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
	}

	[Fact]
	public async Task Should_Not_Authenticate_With_Invalid_Credentials_Test()
	{
		// Arrange
		var request = new AuthenticateRequest("admin", "123123");

		// Act
		var response = await _client.PostAsJsonAsync(_endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}
}
