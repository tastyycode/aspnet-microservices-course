namespace Discount.API.Dtos
{
    public class UpsertCouponDto
    {
        public string ProductName { get; set; }
        public string Description { get; set; }
        public int Amount { get; set; }
    }
}
