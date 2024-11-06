using CommerceMono.Application.Users.Dtos;
using CommerceMono.Application.Users.Features.UpdatingUser.V1;
using CommerceMono.IntegrationTests.Utilities;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using static Bogus.DataSets.Name;

namespace CommerceMono.IntegrationTests.Users;

public class UpdateUserTestBase : AppTestBase
{
	protected override string EndpointName { get; } = "user";

	protected UpdateUserTestBase(
		ITestOutputHelper testOutputHelper,
		TestContainers testContainers
	) : base(testOutputHelper, testContainers)
	{
	}
}

public class UpdateUser_Tests : UpdateUserTestBase
{
	public UpdateUser_Tests(
		ITestOutputHelper testOutputHelper,
		TestContainers testContainers
	) : base(testOutputHelper, testContainers)
	{
	}

	[Fact]
	public async Task Should_Update_User_Test()
	{
		// Arrange
		HttpClient? client = await ApiFactory.LoginAsAdmin();
		var totalCount = await DbContext.Users.CountAsync();
		var testUser = new Faker<EditUserDto>()
			.RuleFor(x => x.Id, 2)
			.RuleFor(u => u.FirstName, (f) => f.Name.FirstName(Gender.Male))
			.RuleFor(u => u.LastName, (f) => f.Name.LastName(Gender.Female))
			.RuleFor(x => x.UserName, f => f.Internet.UserName())
			.RuleFor(x => x.Email, f => f.Internet.Email())
			.RuleFor(x => x.Password, f => f.Internet.Password())
			.RuleFor(x => x.ConfirmPassword, (f, u) => u.Password);
		var request = testUser.Generate();

		// Act
		var response = await client.PostAsJsonAsync(Endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var createResult = await response.Content.ReadFromJsonAsync<UpdateUserResult>();
		createResult.Should().NotBeNull();
		createResult!.User.Should().NotBeNull();
		createResult!.User.Id.Should().Be(2);
		createResult!.User.UserName.Should().Be(request.UserName);
		createResult!.User.FirstName.Should().Be(request.FirstName);
		createResult!.User.LastName.Should().Be(request.LastName);
		createResult!.User.Email.Should().Be(request.Email);

		var newTotalCount = await DbContext.Users.CountAsync();
		newTotalCount.Should().Be(totalCount);
	}
}
