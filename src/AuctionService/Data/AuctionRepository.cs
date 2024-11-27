using System;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class AuctionRepository : IAuctionRepository
{
    private AuctionDbContext _context;
    private IMapper _mapper;

    public AuctionRepository(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public void AddAuction(Auction auction)
    {
        _context.Auctions.Add(auction);
    }

    public async Task<AuctionsDto> GetAuctionByIdAsync(Guid id)
    {
        return await _context.Auctions.ProjectTo<AuctionsDto>(_mapper.ConfigurationProvider).FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Auction> GetAuctionEntityById(Guid id)
    {
        return _mapper.Map<Auction>(await _context.Auctions.Where(x => x.Id == id).Include(x => x.Item).FirstOrDefaultAsync());
    }

    public async Task<List<AuctionsDto>> GetAuctionsAsync(string date)
    {
        var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }

        return await query.ProjectTo<AuctionsDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    public void RemoveAuction(Auction auction)
    {
        _context.Auctions.Remove(auction);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
