using AutoMapper;
using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using EventBus.Messages.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Basket.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository _repository;
        private readonly DiscountGrpcService _discountGrpcService;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<BasketController> _logger;
        private readonly IMapper _mapper;

        public BasketController(
            IBasketRepository repository,
            DiscountGrpcService discountGrpcService,
            IMapper mapper,
            IPublishEndpoint publishEndpoint,
            ILogger<BasketController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _discountGrpcService = discountGrpcService ?? throw new ArgumentNullException(nameof(discountGrpcService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("{userName}", Name = "GetBasket")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> GetBasket(string userName)
        {
            var basket = await _repository.GetBasket(userName);
            return Ok(basket ?? new ShoppingCart(userName));
        }

        [HttpPost(Name = "UpdateBasket")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody] ShoppingCart basket)
        {
            // TODO: consume gRPC GetDiscount method from Discount.Grpc project
            // and calculate latest prices of products in basket
            // consume Discount Grpc
            foreach (var item in basket.Items)
            {
                if (item.Discount == null)
                {
                    var coupon = await _discountGrpcService.GetDiscount(item.ProductName);
                    item.Discount = coupon.Amount;
                }
            }
            return Ok(await _repository.UpdateBasket(basket));
        }

        [HttpDelete("{userName}", Name = "DeleteBasket")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> DeleteBasket(string userName)
        {
            _logger.LogInformation($"Deleting <{userName}>'s basket...");
            await _repository.DeleteBasket(userName);
            _logger.LogInformation($"Deleted <{userName}>'s basket.");

            return Ok();
        }

        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
        {
            // get existing basket with total price
            var basket = await _repository.GetBasket(basketCheckout.UserName);
            if (basket == null)
            {
                return BadRequest();
            }

            // Create BasketCheckoutEvent -- Set TotalPrice on BasketCheckoutEvent message
            BasketCheckoutEvent basketCheckoutEvent = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
            basketCheckoutEvent.TotalPrice = basket.TotalPrice;

            // Send checkout event to RabbitMQ
            _logger.LogInformation($"Publishing {basketCheckoutEvent.GetType()} to Event Bus...");
            await _publishEndpoint.Publish(basketCheckoutEvent);
            _logger.LogInformation($"Published successfully {basketCheckoutEvent.GetType()} to Event Bus.");

            // Remove the basket
            _logger.LogInformation($"Deleting <{basketCheckout.UserName}>'s basket...");
            await _repository.DeleteBasket(basketCheckout.UserName);
            _logger.LogInformation($"Deleted <{basketCheckout.UserName}>'s basket.");

            return Accepted();
        }
    }
}
