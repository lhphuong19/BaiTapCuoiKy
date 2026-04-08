using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020337.BusinessLayers;
using SV22T1020337.Models.Sales;
using SV22T1020337.Shop.Models;

namespace SV22T1020337.Shop.Controllers
{
    public class CartController : Controller
    {
        // ==================== XEM GIỎ HÀNG ====================
        public IActionResult Index()
        {
            try
            {
                var cart = ShoppingCartService.GetShoppingCart();
                return View(cart);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                ModelState.AddModelError("", "Không thể tải giỏ hàng.");
                return View(new List<OrderDetailViewInfo>());
            }
        }

        // ==================== THÊM VÀO GIỎ HÀNG ====================
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] CartItemModel item)
        {
            try
            {
                if (item == null)
                {
                    ModelState.AddModelError("", "Dữ liệu không hợp lệ.");
                    return BadRequest(ModelState);
                }

                var product = await CatalogDataService.GetProductAsync(item.ProductID);
                if (product == null)
                {
                    ModelState.AddModelError("", "Sản phẩm không tồn tại.");
                    return BadRequest(ModelState);
                }

                var cartItem = new OrderDetailViewInfo()
                {
                    ProductID = item.ProductID,
                    ProductName = product.ProductName,
                    Photo = product.Photo ?? "",
                    Unit = product.Unit,
                    Quantity = item.Quantity < 1 ? 1 : item.Quantity,
                    SalePrice = item.SalePrice > 0 ? item.SalePrice : product.Price
                };

                ShoppingCartService.AddCartItem(cartItem);

                int cartCount = ShoppingCartService.GetShoppingCart().Count;

                return Json(new
                {
                    success = true,
                    message = "Đã thêm vào giỏ hàng!",
                    cartCount
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                ModelState.AddModelError("", "Đã xảy ra lỗi khi thêm vào giỏ hàng.");

                return Json(new
                {
                    success = false,
                    message = ModelState.Values
                                        .SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .FirstOrDefault()
                });
            }
        }

        // ==================== CẬP NHẬT GIỎ HÀNG ====================
        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity, decimal salePrice)
        {
            try
            {
                if (productId <= 0)
                {
                    ModelState.AddModelError("", "Sản phẩm không hợp lệ.");
                    return BadRequest(ModelState);
                }

                if (quantity < 1) quantity = 1;

                ShoppingCartService.UpdateCartItem(productId, quantity, salePrice);

                var cart = ShoppingCartService.GetShoppingCart();
                decimal total = cart.Sum(c => c.Quantity * c.SalePrice);
                int count = cart.Count;

                return Json(new { success = true, total, count });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                ModelState.AddModelError("", "Không thể cập nhật giỏ hàng.");

                return Json(new
                {
                    success = false,
                    message = ModelState.Values
                                        .SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .FirstOrDefault()
                });
            }
        }

        // ==================== XÓA KHỎI GIỎ HÀNG ====================
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            try
            {
                if (productId <= 0)
                {
                    ModelState.AddModelError("", "Sản phẩm không hợp lệ.");
                    return BadRequest(ModelState);
                }

                ShoppingCartService.RemoveCartItem(productId);

                var cart = ShoppingCartService.GetShoppingCart();
                decimal total = cart.Sum(c => c.Quantity * c.SalePrice);

                return Json(new
                {
                    success = true,
                    cartCount = cart.Count,
                    total
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                ModelState.AddModelError("", "Không thể xóa sản phẩm.");

                return Json(new
                {
                    success = false,
                    message = ModelState.Values
                                        .SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .FirstOrDefault()
                });
            }
        }

        // ==================== XÓA TOÀN BỘ GIỎ HÀNG ====================
        [HttpPost]
        public IActionResult ClearCart()
        {
            try
            {
                ShoppingCartService.ClearCart();

                return Json(new
                {
                    success = true,
                    message = "Đã xóa toàn bộ giỏ hàng."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                ModelState.AddModelError("", "Không thể xóa giỏ hàng.");

                return Json(new
                {
                    success = false,
                    message = ModelState.Values
                                        .SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .FirstOrDefault()
                });
            }
        }

        // ==================== SỐ LƯỢNG GIỎ HÀNG ====================
        public IActionResult CartCount()
        {
            try
            {
                int count = ShoppingCartService.GetShoppingCart().Count;

                return Json(new { count });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                ModelState.AddModelError("", "Không thể lấy số lượng giỏ hàng.");

                return Json(new
                {
                    count = 0,
                    message = ModelState.Values
                                        .SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .FirstOrDefault()
                });
            }
        }
    }
}