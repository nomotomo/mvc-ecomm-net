using Microsoft.EntityFrameworkCore;
using Ordering.Core.Entities;
using Ordering.Core.Repositories;
using Ordering.Infrastructure.Data;

namespace Ordering.Infrastructure.Repositories
{
    public class OrderRepository : RepositoryBase<Order>, IOrderRepository
    {
        private readonly OrderContext _orderContext;

        public OrderRepository(OrderContext dbContext) : base(dbContext)
        {
            _orderContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return new List<Order>();
            }

            var orderList = await _orderContext.Orders
                .Where(o => o.UserName == userName)
                .ToListAsync();

            return orderList;
        }
    }
}