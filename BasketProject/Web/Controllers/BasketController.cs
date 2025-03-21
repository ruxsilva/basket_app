using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using BasketService.Application.DTO;
using BasketService.Application.Interfaces;
using BasketService.Application.Mappers;
using Microsoft.AspNetCore.Authorization;

namespace BasketService.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BasketController(IBasketService basketService, IBasketItemRepository basketItemRepository, IBasketHistoryService basketHistoryService)
        : ControllerBase
    {
        [HttpPost("process")]
        public async Task<ActionResult<ReceiptDto>> ProcessBasket([FromBody] BasketDto? basketDto)
        {
            if (basketDto == null || basketDto.Items.Count == 0)
            {
                return BadRequest("Basket cannot be empty");
            }
            
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("User ID not found in token");
                }

                var userId = int.Parse(userIdClaim.Value);

                var basket = await basketDto.ToBasketDomainModelAsync(basketItemRepository);
                var receipt = basketService.ProcessBasket(basket);
        
                await basketHistoryService.SaveBasketHistoryAsync(receipt, userId);
        
                return Ok(receipt.ToReceiptDto());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [Authorize]
        [HttpGet("items")]
        public async Task<ActionResult> GetAvailableItems()
        {
            var items = await basketItemRepository.GetAllItemsAsync();
            
            return Ok(items.ToItemDtos());
        }
    }
}