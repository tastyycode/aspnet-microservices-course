using System.Threading.Tasks;

using Discount.Grpc.Dtos;
using Discount.Grpc.Entities;

namespace Discount.Grpc.Repositories
{
    public interface IDiscountRepository
    {
        Task<Coupon> GetDiscount(string productName);
        Task<bool> CreateDiscount(UpsertCouponDto discount);
        Task<bool> UpdateDiscount(UpsertCouponDto discount);
        Task<bool> DeleteDiscount(string productName);
    }
}
