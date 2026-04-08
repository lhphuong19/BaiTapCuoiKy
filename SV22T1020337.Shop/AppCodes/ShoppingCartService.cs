using SV22T1020337.Models.Sales;

namespace SV22T1020337.Shop
{
    /// <summary>
    /// Cung cấp các chức năng xử lý trên giỏ hàng (lưu trong session)
    /// </summary>
    public static class ShoppingCartService
    {
        private const string CART = "ShoppingCart";

        public static List<OrderDetailViewInfo> GetShoppingCart()
        {
            var cart = ApplicationContext.GetSessionData<List<OrderDetailViewInfo>>(CART);
            if (cart == null)
            {
                cart = new List<OrderDetailViewInfo>();
                ApplicationContext.SetSessionData(CART, cart);
            }
            return cart;
        }

        public static OrderDetailViewInfo? GetCartItem(int productID)
            => GetShoppingCart().Find(m => m.ProductID == productID);

        public static void AddCartItem(OrderDetailViewInfo item)
        {
            var cart      = GetShoppingCart();
            var existing  = cart.Find(m => m.ProductID == item.ProductID);
            if (existing == null)
                cart.Add(item);
            else
            {
                existing.Quantity  += item.Quantity;
                existing.SalePrice  = item.SalePrice;
            }
            ApplicationContext.SetSessionData(CART, cart);
        }

        public static void UpdateCartItem(int productID, int quantity, decimal salePrice)
        {
            var cart = GetShoppingCart();
            var item = cart.Find(m => m.ProductID == productID);
            if (item != null)
            {
                item.Quantity  = quantity;
                item.SalePrice = salePrice;
                ApplicationContext.SetSessionData(CART, cart);
            }
        }

        public static void RemoveCartItem(int productID)
        {
            var cart  = GetShoppingCart();
            int index = cart.FindIndex(m => m.ProductID == productID);
            if (index >= 0)
            {
                cart.RemoveAt(index);
                ApplicationContext.SetSessionData(CART, cart);
            }
        }

        public static void ClearCart()
            => ApplicationContext.SetSessionData(CART, new List<OrderDetailViewInfo>());
    }
}
