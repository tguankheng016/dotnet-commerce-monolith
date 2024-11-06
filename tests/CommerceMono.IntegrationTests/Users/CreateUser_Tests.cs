using System.Collections;
using CommerceMono.Application.Users.Dtos;
using CommerceMono.Application.Users.Features.CreatingUser.V1;
using CommerceMono.Application.Users.Models;
using CommerceMono.IntegrationTests.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Bogus.DataSets.Name;

namespace CommerceMono.IntegrationTests.Users;

public class CreateUserTestBase : AppTestBase
{
	protected override string EndpointName { get; } = "user";

	protected CreateUserTestBase(TestWebApplicationFactory apiFactory) : base(apiFactory)
	{
	}
}

public class CreateUser_Tests : CreateUserTestBase
{
	public CreateUser_Tests(TestWebApplicationFactory apiFactory) : base(apiFactory)
	{
	}

	[Fact]
	public async Task Should_Create_User_Test()
	{
		// Arrange
		HttpClient? client = await ApiFactory.LoginAsAdmin();
		var totalCount = await DbContext.Users.CountAsync();
		var testUser = new Faker<CreateUserDto>()
			.RuleFor(x => x.Id, 0)
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

		var createResult = await response.Content.ReadFromJsonAsync<CreateUserResult>();
		createResult.Should().NotBeNull();
		createResult!.User.Should().NotBeNull();
		createResult!.User.Id.Should().BeGreaterThan(0);

		var newTotalCount = await DbContext.Users.CountAsync();
		newTotalCount.Should().Be(totalCount + 1);
	}
}

public class CreateUserValidation_Tests : CreateUserTestBase
{
	public CreateUserValidation_Tests(TestWebApplicationFactory apiFactory) : base(apiFactory)
	{
	}

	[Theory]
	[ClassData(typeof(GetValidateUserCreationTestData))]
	public async Task Should_Create_Role_With_Invalid_Input_Test(CreateUserDto request, string errorMessage)
	{
		// Arrange
		HttpClient? client = await ApiFactory.LoginAsAdmin();

		// Act
		var response = await client.PostAsJsonAsync(Endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
		failureResponse!.Detail.Should().Be(errorMessage);
	}

	private static CreateUserDto GetCreateUserRequest(int scenario)
	{
		var testUser = new Faker<CreateUserDto>()
			.RuleFor(x => x.Id, 0)
			.RuleFor(u => u.FirstName, (f) => f.Name.FirstName(Gender.Male))
			.RuleFor(u => u.LastName, (f) => f.Name.LastName(Gender.Female))
			.RuleFor(x => x.UserName, f => f.Internet.UserName())
			.RuleFor(x => x.Email, f => f.Internet.Email())
			.RuleFor(x => x.Password, f => f.Internet.Password())
			.RuleFor(x => x.ConfirmPassword, (f, u) => u.Password);

		switch (scenario)
		{
			case 0:
				{
					// Email Not Filled In
					testUser.RuleFor(u => u.Email, "");
					break;
				}
			case 1:
				{
					// Password Not Filled In
					testUser.RuleFor(u => u.Password, "");
					break;
				}
			case 2:
				{
					// Password not same as confirm password
					testUser.RuleFor(u => u.ConfirmPassword, (f, u) => u.Password + "Wrong");
					break;
				}
			case 3:
				{
					// Invalid Email
					testUser.RuleFor(u => u.Email, f => f.Internet.UserName());
					break;
				}
			case 4:
				{
					// Exceed length
					testUser.RuleFor(u => u.FirstName, f => f.Internet.Password(length: 1000));
					break;
				}
		}

		return testUser.Generate();
	}

	private class GetValidateUserCreationTestData : IEnumerable<object[]>
	{
		public IEnumerator<object[]> GetEnumerator()
		{
			yield return new object[]
			{
				GetCreateUserRequest(0),
				"Please enter the email address"
			};
			yield return new object[]
			{
				GetCreateUserRequest(1),
				"Please enter the password"
			};
			yield return new object[]
			{
				GetCreateUserRequest(2),
				"Passwords should match"
			};
			yield return new object[]
			{
				GetCreateUserRequest(3),
				"Please enter a valid email address"
			};
			yield return new object[]
			{
				GetCreateUserRequest(4),
				$"The first name length cannot exceed {User.MaxFirstNameLength} characters."
			};
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}

public class CreateUserUnauthorized_Tests : CreateUserTestBase
{
	public CreateUserUnauthorized_Tests(TestWebApplicationFactory apiFactory) : base(apiFactory)
	{
	}

	[Fact]
	public async Task Should_Create_Role_With_Unauthorized_Error_Test()
	{
		// Arrange
		HttpClient? client = await ApiFactory.LoginAsUser();
		var testUser = new Faker<CreateUserDto>()
			.RuleFor(x => x.Id, 0)
			.RuleFor(u => u.FirstName, (f) => f.Name.FirstName(Gender.Male))
			.RuleFor(u => u.LastName, (f) => f.Name.LastName(Gender.Female))
			.RuleFor(x => x.UserName, f => f.Internet.UserName())
			.RuleFor(x => x.Email, f => f.Internet.Email())
			.RuleFor(x => x.Password, f => f.Internet.Password());
		var request = testUser.Generate();
		request.ConfirmPassword = request.Password;

		// Act
		var response = await client.PostAsJsonAsync(Endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
	}
}