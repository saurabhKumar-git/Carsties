using System;
using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

[Collection("Shared collection")]
public class AuctionControllerTests : IAsyncLifetime
{
    private CustomWebAppFactory _factory;
    private HttpClient _httpClient;
    private const string GT_ID = "afbee524-5972-4075-8800-7d1f9d7b0a0c";

    public AuctionControllerTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task GetAuctions_ShouldReturn3Auctions()
    {

        //arrange

        //Act
        var response = await _httpClient.GetFromJsonAsync<AuctionsDto>($"api/auctions/{GT_ID}");

        //Assert
        Assert.Equal("GT", response.Model);
        Assert.NotNull(response);
    }

    [Fact]
    public async Task GetAuctionById_WithInValidIdShouldReturnNotFound()
    {

        //arrange

        //Act
        var response = await _httpClient.GetAsync($"api/auctions/{Guid.NewGuid()}");

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAuctionById_WithValidIdShouldReturnAuction()
    {

        //arrange

        //Act
        var response = await _httpClient.GetFromJsonAsync<List<AuctionsDto>>("api/auctions");

        //Assert
        Assert.Equal(3, response.Count);
    }

    [Fact]
    public async Task GetAuctionById_WithInValidIdShouldReturnBadRequest()
    {

        //arrange

        //Act
        var response = await _httpClient.GetAsync($"api/auctions/notAGuid");

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuction_WithoutAuthShouldReturn401()
    {

        //arrange
        var auctions = new CreateAuctionDto { Make = "Ford" };

        //Act
        var response = await _httpClient.PostAsJsonAsync($"api/auctions", auctions);

        //Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuction_WithAuthShouldReturn201()
    {

        //arrange
        var auctions = GetCreateAuctionDto();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        //Act
        var response = await _httpClient.PostAsJsonAsync($"api/auctions", auctions);

        //Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdAuction = await response.Content.ReadFromJsonAsync<AuctionsDto>();
        Assert.Equal("bob", createdAuction.Seller);
    }

    [Fact]
    public async Task CreateAuction_WithInvalidCreateAuctionDto_ShouldReturn400()
    {
        //arrange
        var auctions = GetCreateAuctionDto();
        auctions.Make = null;
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        //Act
        var response = await _httpClient.PostAsJsonAsync($"api/auctions", auctions);

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_WithValidUpdateDtoAndUser_ShouldReturn200()
    {
        //arrange
        var updateAuctionDto = new UpdateAuctionDto { Make = "FordUpdated" };
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        //Act
        var response = await _httpClient.PutAsJsonAsync($"api/auctions/{GT_ID}", updateAuctionDto);

        //Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_WithValidUpdateDtoAndInvalidUser_ShouldReturn403()
    {
        //arrange
        var updateAuctionDto = new UpdateAuctionDto { Make = "FordUpdated" };
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("Test"));

        //Act
        var response = await _httpClient.PutAsJsonAsync($"api/auctions/{GT_ID}", updateAuctionDto);

        //Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
