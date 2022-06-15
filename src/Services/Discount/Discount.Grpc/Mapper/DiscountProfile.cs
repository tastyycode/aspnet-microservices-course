using AutoMapper;
using Discount.Grpc.Dtos;
using Discount.Grpc.Entities;
using Discount.Grpc.Protos;

namespace Discount.Grpc.Mapper
{
    public class DiscountProfile : Profile
    {
        public DiscountProfile()
        {
            CreateMap<UpsertCouponModel, UpsertCouponDto>().ReverseMap();
            CreateMap<Coupon, CouponModel>().ReverseMap();
        }
    }
}
