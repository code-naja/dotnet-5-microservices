﻿namespace Ordering.Infrastructure.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using Ordering.Application.Contracts.Persistence;
    using Ordering.Domain.Entities;
    using Ordering.Infrastructure.Persistence;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class OrderRepository : RepositoryBase<Order>, IOrderRepository
    {
        public OrderRepository(OrderContext orderContext) : base(orderContext)
        {
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserName(string userName, CancellationToken cancellationToken = default)
        {
            var orderList = await orderContext.Orders.Where(x => x.UserName == userName).ToListAsync(cancellationToken);

            return orderList;
        }
    }
}
