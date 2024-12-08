using System;
using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Contracts;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

public class AuctionBusTests : IClassFixture<CustomWebAppFactory>, IAsyncLifetime
{
    private CustomWebAppFactory _factory;
    private HttpClient _httpClient;
    private ITestHarness _testharness;

    public AuctionBusTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
        _testharness = factory.Services.GetTestHarness();
    }

    [Fact]
    public async Task CreateAuction_ShouldPublishToBus()
    {

        //arrange
        var auctions = GetCreateAuctionDto();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        //Act
        var response = await _httpClient.PostAsJsonAsync($"api/auctions", auctions);

        //Assert
        response.EnsureSuccessStatusCode();
        Assert.True(await _testharness.Published.Any<AuctionCreated>());
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        DbHelper.ReInitDbForTests(db);
        return Task.CompletedTask;
    }

    private CreateAuctionDto GetCreateAuctionDto()
    {
        return new CreateAuctionDto
        {
            Make = "Test",
            Model = "TestModel",
            Year = 2020,
            Color = "Lavendar",
            Mileage = 10,
            ImageUrl = "imageUrl",
            ReservePrice = 100
        };
    }
}
