using AutoMapper;
using MediatR;
using Ordering.Application.Contracts.Persistence;
using Ordering.Domain.Entities;

namespace Ordering.Application.Features.Queries.GetOrdersList
{
    public class GetOrdersListQueryHandler : IRequestHandler<GetOrdersListQuery, List<OrdersVm>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public GetOrdersListQueryHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<OrdersVm>> Handle(GetOrdersListQuery request, CancellationToken cancellationToken)
        {
            List<Order> orderList = (await _orderRepository.GetOrdersByUserNameAsync(request.UserName)).ToList();
            List<OrdersVm> ordersVm = _mapper.Map<List<OrdersVm>>(orderList);
            return ordersVm;
        }
    }
}
