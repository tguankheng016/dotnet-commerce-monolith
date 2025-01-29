namespace CommerceMono.IntegrationTests.Identities;

[CollectionDefinition(Name)]
public class IdentityTestCollection1 : ICollectionFixture<TestWebApplicationFactory>
{
    public const string Name = "Identity Integration Test 1";
}
