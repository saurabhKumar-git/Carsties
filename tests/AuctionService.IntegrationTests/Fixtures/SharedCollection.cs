using System;

namespace AuctionService.IntegrationTests.Fixtures;

[CollectionDefinition("Shared collection")]
public class SharedCollection : ICollectionFixture<CustomWebAppFactory>
{

}