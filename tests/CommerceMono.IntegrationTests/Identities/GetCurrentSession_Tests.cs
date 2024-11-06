
using CommerceMono.Application.Identities.Features.GettingCurrentSession.V1;
using CommerceMono.Application.Users.Constants;
using CommerceMono.IntegrationTests.Utilities;
using CommerceMono.Modules.Permissions;

namespace CommerceMono.IntegrationTests.Identities;

public class GetCurrentSession_Tests : AppTestBase
{
	private readonly TestWebApplicationFactory _apiFactory;
	protected override string EndpointName { get; } = "identities/current-session";

	public GetCurrentSession_Tests(TestWebApplicationFactory apiFactory) : base(apiFactory)
	{
		_apiFactory = apiFactory;
	}

	[Theory]
	[InlineData(UserConsts.DefaultUsername.Admin)]
	[InlineData(UserConsts.DefaultUsername.User)]
	[InlineData(null)]
	public async Task Should_Get_User_Session_Test(string username)
	{
		// Arrange
		HttpClient? client;

		if (username != null)
		{
			client = await _apiFactory.LoginAs(username);
		}
		else
		{
			client = Client;
		}

		// Act
		var response = await client.GetAsync(Endpoint);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var currentSession = await response.Content.ReadFromJsonAsync<GetCurrentSessionResult>();
		currentSession.Should().NotBeNull();

		if (username is null)
		{
			currentSession!.User.Should().BeNull();
		}
		else
		{
			currentSession!.User.Should().NotBeNull();
			currentSession!.User!.UserName.Should().Be(username);

			var allPermissions = AppPermissionProvider.GetPermissions();

			currentSession.AllPermissions.Count().Should().Be(allPermissions.Count());

			if (currentSession.User.UserName == UserConsts.DefaultUsername.Admin)
			{
				currentSession.GrantedPermissions.Count().Should().Be(allPermissions.Count());
			}
		}
	}
}
