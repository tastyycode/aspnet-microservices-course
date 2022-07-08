using Ordering.Domain.Entities;
using Ordering.Application.Contracts.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ordering.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ordering.Infrastructure.Repositories
{
    public class OrderRepository : RepositoryBase<Order>, IOrderRepository
    {
        public OrderRepository(OrderContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserNameAsync(string userName)
        {
            var ordersList = await _dbContext.Orders
                .Where(o => o.UserName == userName)
                .ToListAsync();

            return ordersList;
        }
    }
}
