
using System.Net;
using System.Net.Http.Json;
using CommerceMono.Application.Identities.Features.GettingCurrentSession.V1;
using CommerceMono.Application.Users.Constants;
using CommerceMono.IntegrationTests.Utilities;
using FluentAssertions;

namespace CommerceMono.IntegrationTests.Identities;

public class GetCurrentSession_Tests : IClassFixture<TestWebApplicationFactory>
{
	private readonly TestWebApplicationFactory _apiFactory;
	private readonly string _endpoint = "api/v1/identities/current-session";

	public GetCurrentSession_Tests(TestWebApplicationFactory apiFactory)
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
			client = _apiFactory.CreateClient();
		}

		// Act
		var response = await client.GetAsync(_endpoint);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var currentSession = await response.Content.ReadFromJsonAsync<GetCurrentSessionResult>();
		currentSession.Should().NotBeNull();

		if (username == null)
		{
			currentSession!.User.Should().BeNull();
		}
		else
		{
			currentSession!.User.Should().NotBeNull();
			currentSession!.User!.UserName.Should().Be(username);
		}
	}
}
