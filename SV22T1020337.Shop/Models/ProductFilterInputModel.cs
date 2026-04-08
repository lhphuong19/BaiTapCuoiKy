namespace SV22T1020337.Shop.Models
{
    public class ProductFilterInputModel
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public string? SearchValue { get; set; }
        public int CategoryID { get; set; } = 0;
        public decimal MinPrice { get; set; } = 0;
        public decimal MaxPrice { get; set; } = 0;
    }
}
