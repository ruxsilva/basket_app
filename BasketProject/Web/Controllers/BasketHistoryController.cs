using System.Security.Claims;
using BasketService.Application.DTO;
using BasketService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasketService.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/basket/history")]
    public class BasketHistoryController(IBasketHistoryService basketHistoryService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> GetUserBasketHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("User ID not found in token");
                }

                var userId = int.Parse(userIdClaim.Value);
                
                var paginatedHistory = await basketHistoryService.GetUserBasketHistoryPagedAsync(userId, page, pageSize);
                
                var historyDtos = paginatedHistory.Items.Select(h => new BasketHistoryDto
                {
                    Id = h.Id,
                    CreatedAt = h.CreatedAt,
                    TotalAmount = h.TotalAmount,
                    TotalDiscount = h.TotalDiscount,
                    FinalAmount = h.FinalAmount,
                    Items = h.Items.Select(i => new BasketHistoryItemDto
                    {
                        ItemName = i.ItemName,
                        ItemPrice = i.ItemPrice,
                        Quantity = i.Quantity,
                        LineTotal = i.LineTotal
                    }).ToList()
                }).ToList();
                
                var response = new PaginatedResponse<BasketHistoryDto>
                {
                    Items = historyDtos,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalCount = paginatedHistory.TotalCount,
                    TotalPages = (int)Math.Ceiling(paginatedHistory.TotalCount / (double)pageSize)
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}