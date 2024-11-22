using System;
using AuctionService.Data;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class BidsConsumer : IConsumer<BidPlaced>
{
    private AuctionDbContext _auctionDbContext;

    public BidsConsumer(AuctionDbContext auctionDbContext)
    {
        _auctionDbContext = auctionDbContext;
    }
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("---> Bids Placed --->");

        var auction = await _auctionDbContext.Auctions.FindAsync(context.Message.AuctionId);

        if (auction != null)
        {

            if (auction.CurrentHighBid != null || (context.Message.BidStatus.Contains("Accepted") && context.Message.Amount > auction.CurrentHighBid))
            {
                auction.CurrentHighBid = context.Message.Amount;
            }

            await _auctionDbContext.SaveChangesAsync();
        }

    }
}
