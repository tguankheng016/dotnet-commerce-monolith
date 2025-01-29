namespace CommerceMono.IntegrationTests.Roles;

[CollectionDefinition(Name)]
public class RoleTestCollection1 : ICollectionFixture<TestWebApplicationFactory>
{
    public const string Name = "Role Integration Test 1";
}
