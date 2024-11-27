using System;
using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.Helpers;
using AuctionService.UnitTests.Utils;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuctionService.UnitTests;

public class AuctionControllerTests
{
    private readonly Mock<IAuctionRepository> _auctionRepo;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly IMapper _mapper;
    private readonly Fixture _fixture;
    private readonly AuctionsController _controller;
    public AuctionControllerTests()
    {
        _fixture = new Fixture();
        _auctionRepo = new Mock<IAuctionRepository>();
        _publishEndpoint = new Mock<IPublishEndpoint>();
        var mockMapper = new MapperConfiguration(mc =>
        {
            mc.AddMaps(typeof(MappingProfiles).Assembly);
        }).CreateMapper().ConfigurationProvider;

        _mapper = new Mapper(mockMapper);
        _controller = new AuctionsController(_auctionRepo.Object, _mapper, _publishEndpoint.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = Helper.GetClaimsPrincipal() }
            }
        };
    }

    [Fact]
    public async Task GetAuctions_WithNoParams_Returns10Auctions()
    {
        //Arrange
        var auctions = _fixture.CreateMany<AuctionsDto>(10).ToList();

        _auctionRepo.Setup(x => x.GetAuctionsAsync(null)).ReturnsAsync(auctions);

        //Act
        var result = await _controller.GetAllAuctions(null);

        // Assert
        Assert.Equal(10, result.Value.Count);
        Assert.IsType<ActionResult<IList<AuctionsDto>>>(result);
    }

    [Fact]
    public async Task GetAuctionsById_WithValidGuid_ReturnsAuction()
    {
        //Arrange
        var auction = _fixture.Create<AuctionsDto>();

        _auctionRepo.Setup(x => x.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

        //Act
        var result = await _controller.GetAuctionDetails(auction.Id);

        // Assert
        Assert.IsType<ActionResult<AuctionsDto>>(result);
    }

    [Fact]
    public async Task GetAuctionsById_WithInValidGuid_ReturnsNotFound()
    {
        //Arrange
        var auction = _fixture.Create<AuctionsDto>();

        _auctionRepo.Setup(x => x.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);

        //Act
        var result = await _controller.GetAuctionDetails(auction.Id);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateAuction_WithValidRequest_ReturnsCreatedAtActionResult()
    {
        //Arrange
        var auction = _fixture.Create<CreateAuctionDto>();

        _auctionRepo.Setup(x => x.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

        //Act
        var result = await _controller.CreateAuction(auction);
        var createdAtAction = result.Result as CreatedAtActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("GetAuctionDetails", createdAtAction.ActionName);
        Assert.IsType<AuctionsDto>(createdAtAction.Value);
    }

    [Fact]
    public async Task CreateAuction_FailedSave_Returns400BadRequest()
    {
        //Arrange
        var auction = _fixture.Create<CreateAuctionDto>();

        _auctionRepo.Setup(x => x.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(x => x.SaveChangesAsync()).ReturnsAsync(false);

        //Act
        var result = await _controller.CreateAuction(auction);

        // Assert
        Assert.NotNull(result.Result);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
    {
        //Arrange
        var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
        auction.Seller = "test";

        _auctionRepo.Setup(x => x.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(x => x.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

        //Act
        var result = await _controller.UpdateAuction(It.IsAny<Guid>(), updateAuctionDto);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidUser_Returns403Forbid()
    {
        //Arrange
        var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();

        _auctionRepo.Setup(x => x.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(x => x.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

        //Act
        var result = await _controller.UpdateAuction(It.IsAny<Guid>(), updateAuctionDto);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
    {
        //Arrange
        var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();


        _auctionRepo.Setup(x => x.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(value: null);

        //Act
        var result = await _controller.UpdateAuction(It.IsAny<Guid>(), updateAuctionDto);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidUser_Returns403Response()
    {
        throw new NotImplementedException();
    }
}
