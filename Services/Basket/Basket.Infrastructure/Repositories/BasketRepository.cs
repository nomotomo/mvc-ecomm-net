using Basket.Core.Entities;
using Basket.Core.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Basket.Infrastructure.Repositories;

public class BasketRepository : IBasketRepository
{
    private readonly IDistributedCache _redisCache;
    public BasketRepository(IDistributedCache redisCache)
    {
        _redisCache = redisCache;
    }
    
    public async Task<ShoppingCart> GetBasket(string username)
    {
        var basket = await _redisCache.GetStringAsync(username);
        if (String.IsNullOrEmpty(basket))
        {
            return await Task.FromResult<ShoppingCart>(null);
        }
        
        return JsonConvert.DeserializeObject<ShoppingCart>(basket);
    }

    public async Task<ShoppingCart> UpdateBasket(ShoppingCart basket)
    {
        await _redisCache.SetStringAsync(basket.Username, JsonConvert.SerializeObject(basket));
        return await Task.FromResult<ShoppingCart>(basket);
    }

    public async Task DeleteBasket(string username)
    {
        await _redisCache.RemoveAsync(username);
    }
}