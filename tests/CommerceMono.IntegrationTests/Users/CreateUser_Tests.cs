using System.Collections;
using CommerceMono.Application.Users.Constants;
using CommerceMono.Application.Users.Dtos;
using CommerceMono.Application.Users.Features.CreatingUser.V1;
using CommerceMono.Application.Users.Models;
using CommerceMono.IntegrationTests.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using static Bogus.DataSets.Name;

namespace CommerceMono.IntegrationTests.Users;

[Collection(UserTestCollection1.Name)]
public class CreateUserTestBase : AppTestBase
{
	protected override string EndpointName { get; } = "user";

	protected CreateUserTestBase(
		ITestOutputHelper testOutputHelper,
		TestWebApplicationFactory webAppFactory
	) : base(testOutputHelper, webAppFactory)
	{
	}
}

public class CreateUser_Tests : CreateUserTestBase
{
	public CreateUser_Tests(
		ITestOutputHelper testOutputHelper,
		TestWebApplicationFactory webAppFactory
	) : base(testOutputHelper, webAppFactory)
	{
	}

	[Fact]
	public async Task Should_Create_User_Test()
	{
		// Arrange
		var client = await ApiFactory.LoginAsAdmin();
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

		// TODO: Soft Delete Violate Unique Index Of Username
		// Create a deleted user with the same username and email
		// await DbContext.Users.AddAsync(new User()
		// {
		// 	FirstName = request.FirstName!,
		// 	LastName = request.LastName!,
		// 	UserName = request.UserName,
		// 	NormalizedUserName = request.UserName!.ToUpper(),
		// 	Email = request.Email,
		// 	NormalizedEmail = request.Email!.ToUpper(),
		// 	IsDeleted = true
		// });

		// await DbContext.SaveChangesAsync();

		// Act
		var response = await client.PostAsJsonAsync(Endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var createResult = await response.Content.ReadFromJsonAsync<CreateUserResult>();
		createResult.Should().NotBeNull();
		createResult!.User.Should().NotBeNull();
		createResult!.User.Id.Should().BeGreaterThan(0);
		createResult!.User.UserName.Should().Be(request.UserName);
		createResult!.User.FirstName.Should().Be(request.FirstName);
		createResult!.User.LastName.Should().Be(request.LastName);
		createResult!.User.Email.Should().Be(request.Email);

		var newTotalCount = await DbContext.Users.CountAsync();
		newTotalCount.Should().Be(totalCount + 1);
	}

	[Theory]
	[InlineData(null, "admin@testgk.com", "Email 'admin@testgk.com' is already taken.")]
	[InlineData(UserConsts.DefaultUsername.User, null, "Username 'gkuser1' is already taken.")]
	public async Task Should_Not_Create_User_With_Duplicate_Username_Or_Email_Test(string? username, string? email, string errorMessage)
	{
		// Arrange
		var client = await ApiFactory.LoginAsAdmin();
		var testUser = new Faker<CreateUserDto>()
			.RuleFor(x => x.Id, 0)
			.RuleFor(u => u.FirstName, (f) => f.Name.FirstName(Gender.Male))
			.RuleFor(u => u.LastName, (f) => f.Name.LastName(Gender.Female))
			.RuleFor(x => x.UserName, username)
			.RuleFor(x => x.Email, email)
			.RuleFor(x => x.Password, f => f.Internet.Password())
			.RuleFor(x => x.ConfirmPassword, (f, u) => u.Password);

		if (username is null)
		{
			testUser.RuleFor(x => x.UserName, (f) => f.Internet.UserName());
		}

		if (email is null)
		{
			testUser.RuleFor(x => x.Email, f => f.Internet.Email());
		}

		var request = testUser.Generate();

		// Act
		var response = await client.PostAsJsonAsync(Endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
		failureResponse!.Detail.Should().Be(errorMessage);
	}

	[Fact]
	public async Task Should_Create_User_With_Unauthorized_Error_Test()
	{
		// Arrange
		var client = await ApiFactory.LoginAsUser();
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

public class CreateUserValidation_Tests : CreateUserTestBase
{
	public CreateUserValidation_Tests(
		ITestOutputHelper testOutputHelper,
		TestWebApplicationFactory webAppFactory
	) : base(testOutputHelper, webAppFactory)
	{
	}

	[Theory]
	[ClassData(typeof(GetValidateUserCreationTestData))]
	public async Task Should_Create_User_With_Invalid_Input_Test(CreateUserDto request, string errorMessage)
	{
		// Arrange
		var client = await ApiFactory.LoginAsAdmin();

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
			case 5:
				{
					// Invalid Id
					testUser.RuleFor(u => u.Id, 1);
					break;
				}
			case 6:
				{
					// Invalid Id
					testUser.RuleFor(u => u.Id, -5);
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
			yield return new object[]
			{
				GetCreateUserRequest(5),
				"Invalid user id"
			};
			yield return new object[]
			{
				GetCreateUserRequest(6),
				"Invalid user id"
			};
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
