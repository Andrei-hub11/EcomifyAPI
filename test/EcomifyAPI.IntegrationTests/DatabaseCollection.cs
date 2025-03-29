using EcomifyAPI.IntegrationTests.Fixture;

namespace EcomifyAPI.IntegrationTests;

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<AppHostFixture>
{
    // This class has no code, and is never created. Its purpose is to be the place to apply [CollectionDefinition]
}