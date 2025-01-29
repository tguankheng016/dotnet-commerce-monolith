using CommerceMono.Application.Roles.Models;

namespace CommerceMono.IntegrationTests.Roles;

public static class RoleFaker
{
    public static Faker<Role> GetRoleFaker()
    {
        var testRole = new Faker<Role>()
            .RuleFor(x => x.Id, 0)
            .RuleFor(u => u.Name, (f) => f.Name.JobArea())
            .RuleFor(x => x.NormalizedName, (f, u) => u.Name!.ToUpper());

        return testRole;
    }
}