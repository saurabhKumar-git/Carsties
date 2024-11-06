using System;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;

namespace AuctionService.Helpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Auction, AuctionsDto>().IncludeMembers(x => x.Item);
        CreateMap<Item, AuctionsDto>();
        CreateMap<CreateAuctionDto, Auction>().ForMember(d => d.Item, o => o.MapFrom(x => x));
        CreateMap<CreateAuctionDto, Item>();
        CreateMap<AuctionsDto, AuctionCreated>();
    }
}
