using CommerceMono.Application.Users.Models;
using static Bogus.DataSets.Name;

namespace CommerceMono.IntegrationTests.Users;

public static class UserFaker
{
    public static Faker<User> GetUserFaker()
    {
        var testUser = new Faker<User>()
            .RuleFor(x => x.Id, 0)
            .RuleFor(u => u.FirstName, (f) => f.Name.FirstName(Gender.Male))
            .RuleFor(u => u.LastName, (f) => f.Name.LastName(Gender.Female))
            .RuleFor(x => x.UserName, f => f.Internet.UserName())
            .RuleFor(x => x.NormalizedUserName, (f, u) => u.UserName!.ToUpper())
            .RuleFor(x => x.Email, f => f.Internet.Email())
            .RuleFor(x => x.NormalizedEmail, (f, u) => u.Email!.ToUpper())
            .RuleFor(x => x.SecurityStamp, Guid.NewGuid().ToString());

        return testUser;
    }
}