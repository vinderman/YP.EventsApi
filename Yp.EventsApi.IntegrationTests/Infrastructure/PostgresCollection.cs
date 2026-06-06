namespace Yp.EventsApi.IntegrationTests.Infrastructure;

[CollectionDefinition(nameof(PostgresCollection))]
public class PostgresCollection : ICollectionFixture<PostgresFixture>;
