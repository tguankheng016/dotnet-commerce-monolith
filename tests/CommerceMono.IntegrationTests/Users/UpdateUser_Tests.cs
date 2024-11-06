using System.Collections;
using CommerceMono.Application.Users.Dtos;
using CommerceMono.Application.Users.Features.UpdatingUser.V1;
using CommerceMono.Application.Users.Models;
using CommerceMono.IntegrationTests.Utilities;
using Microsoft.AspNetCore.Mvc;
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
		var response = await client.PutAsJsonAsync(Endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var updateResult = await response.Content.ReadFromJsonAsync<UpdateUserResult>();
		updateResult.Should().NotBeNull();
		updateResult!.User.Should().NotBeNull();
		updateResult!.User.Id.Should().Be(2);
		updateResult!.User.UserName.Should().Be(request.UserName);
		updateResult!.User.FirstName.Should().Be(request.FirstName);
		updateResult!.User.LastName.Should().Be(request.LastName);
		updateResult!.User.Email.Should().Be(request.Email);

		// TODO: Check User Role Cache

		var newTotalCount = await DbContext.Users.CountAsync();
		newTotalCount.Should().Be(totalCount);
	}

	// TODO: Add Test For Duplicate Username and Email

	[Fact]
	public async Task Should_Update_Role_With_Unauthorized_Error_Test()
	{
		// Arrange
		HttpClient? client = await ApiFactory.LoginAsUser();
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
		var response = await client.PutAsJsonAsync(Endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
	}
}

public class UpdateUserValidation_Tests : UpdateUserTestBase
{
	public UpdateUserValidation_Tests(
		ITestOutputHelper testOutputHelper,
		TestContainers testContainers
	) : base(testOutputHelper, testContainers)
	{
	}

	[Theory]
	[ClassData(typeof(GetValidateUserUpdateTestData))]
	public async Task Should_Update_Role_With_Invalid_Input_Test(EditUserDto request, string errorMessage)
	{
		// Arrange
		HttpClient? client = await ApiFactory.LoginAsAdmin();

		// Act
		var response = await client.PutAsJsonAsync(Endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

		var failureResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		failureResponse.Should().NotBeNull();
		failureResponse!.Detail.Should().Be(errorMessage);
	}

	private static EditUserDto GetUpdateUserRequest(int scenario)
	{
		var testUser = new Faker<EditUserDto>()
			.RuleFor(x => x.Id, 2)
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

	private class GetValidateUserUpdateTestData : IEnumerable<object[]>
	{
		public IEnumerator<object[]> GetEnumerator()
		{
			yield return new object[]
			{
				GetUpdateUserRequest(0),
				"Please enter the email address"
			};
			yield return new object[]
			{
				GetUpdateUserRequest(2),
				"Passwords should match"
			};
			yield return new object[]
			{
				GetUpdateUserRequest(3),
				"Please enter a valid email address"
			};
			yield return new object[]
			{
				GetUpdateUserRequest(4),
				$"The first name length cannot exceed {User.MaxFirstNameLength} characters."
			};
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
