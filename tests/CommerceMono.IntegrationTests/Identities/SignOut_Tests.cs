using CommerceMono.Application.Identities.Features.Authenticating.V2;
using CommerceMono.Application.Users.Constants;
using CommerceMono.Modules.Security;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace CommerceMono.IntegrationTests.Identities;

[Collection(IdentityTestCollection1.Name)]
public class SignOut_Tests : AppTestBase
{
	protected override string EndpointName { get; } = "identities/sign-out";

	public SignOut_Tests(
		ITestOutputHelper testOutputHelper,
		TestWebApplicationFactory webAppFactory
	) : base(testOutputHelper, webAppFactory)
	{
	}

	[Fact]
	public async Task Should_SignOut_Success_Tests()
	{
		// Arrange
		var request = new AuthenticateRequest(UserConsts.DefaultUsername.Admin, "123qwe");
		var user = await DbContext.Users.FirstAsync(x => x.NormalizedUserName == UserConsts.DefaultUsername.Admin.ToUpper());
		var response = await Client.PostAsJsonAsync("api/v2/identities/authenticate", request);
		var authenticateResponse = await response.Content.ReadFromJsonAsync<AuthenticateResult>();

		Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authenticateResponse!.AccessToken}");

		// Act
		response = await Client.PostAsync(Endpoint, null);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var tokenKeyCaches = await CacheProvider.GetByPrefixAsync<string>($"{TokenConsts.TokenValidityKey}.{user.Id}");
		tokenKeyCaches.Should().NotBeNull();
		tokenKeyCaches!.Count().Should().Be(0);
	}
}

