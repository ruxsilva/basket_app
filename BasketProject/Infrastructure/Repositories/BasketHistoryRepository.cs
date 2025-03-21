using BasketService.Application.Interfaces;
using BasketService.Domain.Models;
using Dapper;
using MySql.Data.MySqlClient;

namespace BasketService.Infrastructure.Repositories;

public class BasketHistoryRepository(IConfiguration configuration) : IBasketHistoryRepository
{
        private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection");

        private MySqlConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public async Task<BasketHistory> SaveBasketHistoryAsync(BasketHistory basketHistory)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            
            // Begin a transaction to ensure all operations complete together
            await using var transaction = await connection.BeginTransactionAsync();
            
            try
            {
                var basketSql = @"
                    INSERT INTO basket_history 
                    (user_id, created_at, total_amount, total_discount, final_amount) 
                    VALUES (@UserId, @CreatedAt, @TotalAmount, @TotalDiscount, @FinalAmount);
                    SELECT LAST_INSERT_ID();";
                
                var basketId = await connection.ExecuteScalarAsync<int>(basketSql, new
                {
                    basketHistory.UserId,
                    basketHistory.CreatedAt,
                    basketHistory.TotalAmount,
                    basketHistory.TotalDiscount,
                    basketHistory.FinalAmount
                }, transaction);
                
                basketHistory.Id = basketId;
                
                if (basketHistory.Items.Any())
                {
                    var itemsSql = @"
                        INSERT INTO basket_history_items 
                        (basket_history_id, item_name, item_price, quantity, line_total) 
                        VALUES (@BasketHistoryId, @ItemName, @ItemPrice, @Quantity, @LineTotal);";
                    
                    foreach (var item in basketHistory.Items)
                    {
                        item.BasketHistoryId = basketId;
                        await connection.ExecuteAsync(itemsSql, item, transaction);
                    }
                }
                
                await transaction.CommitAsync();
                return basketHistory;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PaginatedResult<BasketHistory>> GetUserBasketHistoryPagedAsync(int userId, int page, int pageSize)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            
            var offset = (page - 1) * pageSize;
            
            var countSql = "SELECT COUNT(*) FROM basket_history WHERE user_id = @UserId";
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { UserId = userId });
            
            // Get paginated baskets
            var basketsSql = @"
                SELECT id, user_id as UserId, created_at as CreatedAt, 
                       total_amount as TotalAmount, total_discount as TotalDiscount, 
                       final_amount as FinalAmount 
                FROM basket_history 
                WHERE user_id = @UserId 
                ORDER BY created_at DESC
                LIMIT @PageSize OFFSET @Offset";
            
            var baskets = await connection.QueryAsync<BasketHistory>(basketsSql, 
                new { UserId = userId, PageSize = pageSize, Offset = offset });
            
            var basketsList = baskets.ToList();

            if (!basketsList.Any())
            {
                return new PaginatedResult<BasketHistory>
                {
                    Items = basketsList,
                    TotalCount = totalCount
                };
            }
            
            var basketIds = basketsList.Select(b => b.Id).ToArray();
            var itemsSql = @"
                    SELECT id, basket_history_id as BasketHistoryId, item_name as ItemName, 
                           item_price as ItemPrice, quantity, line_total as LineTotal 
                    FROM basket_history_items 
                    WHERE basket_history_id IN @BasketIds";
                
            var items = await connection.QueryAsync<BasketHistoryItem>(itemsSql, new { BasketIds = basketIds });
            
            var itemsByBasket = items.GroupBy(i => i.BasketHistoryId)
                .ToDictionary(g => g.Key, g => g.ToList());
            
            foreach (var basket in basketsList)
            {
                if (itemsByBasket.TryGetValue(basket.Id, out var basketItems))
                {
                    basket.Items = basketItems;
                }
            }

            return new PaginatedResult<BasketHistory>
            {
                Items = basketsList,
                TotalCount = totalCount
            };
        }
    }