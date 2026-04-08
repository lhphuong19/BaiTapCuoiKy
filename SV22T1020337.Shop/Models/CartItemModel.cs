namespace SV22T1020337.Shop.Models
{
    public class CartItemModel
    {
        public int     ProductID { get; set; }
        public int     Quantity  { get; set; } = 1;
        public decimal SalePrice { get; set; }
    }
}
