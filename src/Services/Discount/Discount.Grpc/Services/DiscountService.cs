using AutoMapper;
using Discount.Grpc.Dtos;
using Discount.Grpc.Protos;
using Discount.Grpc.Repositories;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Discount.Grpc.Services
{
    public class DiscountService : DiscountProtoService.DiscountProtoServiceBase
    {
        private readonly IDiscountRepository _repository;
        private readonly ILogger<DiscountService> _logger;
        private readonly IMapper _mapper;

        public DiscountService(IDiscountRepository repository, ILogger<DiscountService> logger, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
        {
            var coupon = await _repository.GetDiscount(request.ProductName);

            if (coupon == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"Discount with Product Name={request.ProductName} is not found."));

            var couponModel = _mapper.Map<CouponModel>(coupon);

            return couponModel;
        }

        public override async Task<UpsertCouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
        {
            UpsertCouponDto upsertCoupon = _mapper.Map<UpsertCouponDto>(request.Coupon);

            await _repository.CreateDiscount(upsertCoupon);

            _logger.LogInformation($"Discount was created successfully. ProductName: {upsertCoupon.ProductName}");

            return request.Coupon;
        }

        public override async Task<UpsertCouponModel> UpdateDiscount(UpdateDiscountRequest request, ServerCallContext context)
        {
            UpsertCouponDto upsertCoupon = _mapper.Map<UpsertCouponDto>(request.Coupon);

            await _repository.UpdateDiscount(upsertCoupon);

            _logger.LogInformation($"Discount was updated successfully. ProductName: {upsertCoupon.ProductName}");

            return request.Coupon;
        }

        public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request, ServerCallContext context)
        {
            bool success = await _repository.DeleteDiscount(request.ProductName);

            DeleteDiscountResponse response = new DeleteDiscountResponse
            {
                Success = success
            };

            return response;
        }
    }
}
