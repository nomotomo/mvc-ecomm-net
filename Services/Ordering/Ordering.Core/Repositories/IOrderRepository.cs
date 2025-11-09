using System.Linq.Expressions;
using Ordering.Core.Entities;

namespace Ordering.Core.Repositories;

public interface IOrderRepository : IAsyncRepository<Order>
{
    Task<IEnumerable<Order>> GetOrdersByUserName(string userName);
    
    Task AddOutBoxMessageAsync(OutBoxMessage outBoxMessage);
}