using System;
using Contracts;
using AutoMapper;
using SearchService.Models;

namespace SearchService.Helpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<AuctionCreated, Item>();
        CreateMap<AuctionUpdated, Item>();
    }
}
