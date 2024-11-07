using System;
using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
    private IMapper _mapper;

    public AuctionUpdatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        Console.WriteLine("--> Consuming Auction deletion " + context.Message.Id);

        var auction = _mapper.Map<Item>(context.Message);

        await DB.Update<Item>().MatchID(auction.ID).ModifyOnly(x => new { x.Make, x.Model, x.Year, x.Color, x.Mileage }, auction).ExecuteAsync();
    }
}
