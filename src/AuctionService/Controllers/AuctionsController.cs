using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [Route("api/auctions")]
    [ApiController]
    public class AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint) : ControllerBase
    {
        public AuctionDbContext _context = context;
        public IMapper _mapper = mapper;

        public IPublishEndpoint _publishEndpoint = publishEndpoint;

        [HttpGet("healthy")]
        public IActionResult Healthy()
        {
            return Ok("Healthy");
        }

        [HttpGet]
        public async Task<ActionResult<IList<AuctionsDto>>> GetAllAuctions(string? date)
        {
            var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

            if (!string.IsNullOrEmpty(date))
            {
                query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
            }

            return await query.ProjectTo<AuctionsDto>(_mapper.ConfigurationProvider).ToListAsync();
        }

        [HttpGet("{auctionId}")]
        public async Task<ActionResult<AuctionsDto>> GetAuctionDetails(Guid auctionId)
        {
            var auctions = _mapper.Map<AuctionsDto>(await _context.Auctions.Where(x => x.Id == auctionId).Include(x => x.Item).FirstOrDefaultAsync());
            return Ok(auctions);
        }

        [HttpPost]
        public async Task<ActionResult<AuctionsDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = _mapper.Map<Auction>(auctionDto);
            // TODO: Add current user as seller
            auction.Seller = "test";

            _context.Auctions.Add(auction);

            var newAuction = _mapper.Map<AuctionsDto>(auction);

            await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                return BadRequest("Could not create Auction!");
            }

            var auctionId = auction.Id;
            return CreatedAtAction(nameof(GetAuctionDetails), new { auctionId }, newAuction);
        }

        [HttpPut("{auctionId}")]
        public async Task<ActionResult> UpdateAuction(Guid auctionId, UpdateAuctionDto auctionDto)
        {
            var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == auctionId);

            // TODO: Check seller and user name
            auction.Item.Make = auctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = auctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = auctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = auctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = auctionDto.Year ?? auction.Item.Year;

            _context.Update(auction);

            var newAuction = _mapper.Map<AuctionsDto>(auction);

            await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(newAuction));

            var result = await _context.SaveChangesAsync() > 0;
            if (result) return Ok();

            return BadRequest("Could not update the Auction!");
        }

        [HttpDelete("{auctionId}")]
        public async Task<ActionResult> DeleteAuction(Guid auctionId)
        {

            var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == auctionId);

            if (auction == null) return NotFound();

            // TODO: Check seller == username
            _context.Auctions.Remove(auction);

            var newAuction = _mapper.Map<AuctionsDto>(auction);

            await _publishEndpoint.Publish(_mapper.Map<AuctionDeleted>(newAuction));

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Could not delete from DB");

            return Ok();
        }
    }
}
