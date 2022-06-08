using Discount.API.Dtos;
using Discount.API.Entities;
using System.Threading.Tasks;

namespace Discount.API.Repositories
{
    public interface IDiscountRepository
    {
        Task<Coupon> GetDiscount(string productName);
        Task<bool> CreateDiscount(UpsertCouponDto discount);
        Task<bool> UpdateDiscount(UpsertCouponDto discount);
        Task<bool> DeleteDiscount(string productName);
    }
}
