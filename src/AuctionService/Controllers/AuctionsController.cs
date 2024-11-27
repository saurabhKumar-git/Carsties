using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [Route("api/auctions")]
    [ApiController]
    public class AuctionsController(IAuctionRepository auctionRepository, IMapper mapper, IPublishEndpoint publishEndpoint) : ControllerBase
    {
        public IAuctionRepository _auctionRepository = auctionRepository;
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
            return await _auctionRepository.GetAuctionsAsync(date);
        }

        [HttpGet("{auctionId}")]
        public async Task<ActionResult<AuctionsDto>> GetAuctionDetails(Guid auctionId)
        {
            var auctions = await _auctionRepository.GetAuctionByIdAsync(auctionId);

            if (auctions == null)
            {
                return NotFound();
            }
            return Ok(auctions);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AuctionsDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = _mapper.Map<Auction>(auctionDto);
            // TODO: Add current user as seller
            auction.Seller = User.Identity.Name;

            _auctionRepository.AddAuction(auction);

            var newAuction = _mapper.Map<AuctionsDto>(auction);

            await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

            var result = await _auctionRepository.SaveChangesAsync();

            if (!result)
            {
                return BadRequest("Could not create Auction!");
            }

            var auctionId = auction.Id;
            return CreatedAtAction(nameof(GetAuctionDetails), new { auctionId }, newAuction);
        }

        [Authorize]
        [HttpPut("{auctionId}")]
        public async Task<ActionResult> UpdateAuction(Guid auctionId, UpdateAuctionDto auctionDto)
        {
            var auction = await _auctionRepository.GetAuctionEntityById(auctionId);

            if (auction == null)
            {
                return NotFound();
            }

            // TODO: Check seller and user name
            if (auction.Seller != User.Identity.Name)
            {
                return Forbid();
            }
            auction.Item.Make = auctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = auctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = auctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = auctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = auctionDto.Year ?? auction.Item.Year;

            var newAuction = _mapper.Map<AuctionsDto>(auction);

            await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(newAuction));

            var result = await _auctionRepository.SaveChangesAsync();
            if (result) return Ok();

            return BadRequest("Could not update the Auction!");
        }

        [Authorize]
        [HttpDelete("{auctionId}")]
        public async Task<ActionResult> DeleteAuction(Guid auctionId)
        {

            var auction = await _auctionRepository.GetAuctionEntityById(auctionId);

            if (auction == null) return NotFound();

            // TODO: Check seller == username
            if (auction.Seller != User.Identity.Name)
            {
                return Forbid();
            }
            _auctionRepository.RemoveAuction(auction);

            var newAuction = _mapper.Map<AuctionsDto>(auction);

            await _publishEndpoint.Publish(_mapper.Map<AuctionDeleted>(newAuction));

            var result = await _auctionRepository.SaveChangesAsync();

            if (!result) return BadRequest("Could not delete from DB");

            return Ok();
        }
    }
}
