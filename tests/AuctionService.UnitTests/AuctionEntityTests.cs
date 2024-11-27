using System.Data.Common;
using AuctionService.Entities;

namespace AuctionService.UnitTests;

public class AuctionEntityTests
{
    [Fact]
    public void HasReservePrice_ReservPriceGtZero_True()
    {
        //arrange
        var auction = new Auction { Id = Guid.NewGuid(), ReservePrice = 10 };

        // Act
        var result = auction.HasReservePrice();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasReservePrice_ReservPriceIsZero_True()
    {
        //arrange
        var auction = new Auction { Id = Guid.NewGuid(), ReservePrice = 0 };

        // Act
        var result = auction.HasReservePrice();

        // Assert
        Assert.False(result);
    }
}