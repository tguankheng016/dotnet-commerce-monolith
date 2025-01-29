namespace CommerceMono.IntegrationTests.Users;

[CollectionDefinition(Name)]
public class UserTestCollection1 : ICollectionFixture<TestWebApplicationFactory>
{
    public const string Name = "User Integration Test 1";
}