using SV22T1020337.Models.Sales;

namespace SV22T1020337.Shop.Models
{
    /// <summary>
    /// Model tìm kiếm lịch sử đơn hàng dành cho khách hàng (Shop)
    /// </summary>
    public class CustomerOrderSearchInput
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public OrderStatusEnum Status { get; set; } = 0;
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
