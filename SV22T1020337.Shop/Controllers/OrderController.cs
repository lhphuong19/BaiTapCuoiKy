using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020337.BusinessLayers;
using SV22T1020337.Models.Sales;
using SV22T1020337.Shop.Models;

namespace SV22T1020337.Shop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private const string ORDER_SEARCH_KEY = "ShopOrderSearch";

        // ==================== CHECKOUT ====================

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                var cart = ShoppingCartService.GetShoppingCart();
                if (!cart.Any())
                {
                    TempData["Error"] = "Giỏ hàng trống.";
                    return RedirectToAction("Index", "Cart");
                }

                var userData = User.GetUserData();
                if (userData == null) return RedirectToAction("Login", "Account");

                int customerId = int.Parse(userData.UserId!);
                var customer = await PartnerDataService.GetCustomerAsync(customerId);

                var model = new CheckoutModel()
                {
                    DeliveryProvince = customer?.Province ?? "",
                    DeliveryAddress = customer?.Address ?? ""
                };

                ViewBag.Cart = cart;
                ViewBag.Total = cart.Sum(c => c.Quantity * c.SalePrice);

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                ModelState.AddModelError("", "Không thể tải trang thanh toán.");
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutModel model)
        {
            var cart = ShoppingCartService.GetShoppingCart();

            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToAction("Index", "Cart");
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Cart = cart;
                    ViewBag.Total = cart.Sum(c => c.Quantity * c.SalePrice);
                    return View(model);
                }

                var userData = User.GetUserData();
                if (userData == null) return RedirectToAction("Login", "Account");

                int customerId = int.Parse(userData.UserId!);

                int orderId = await SalesDataService.AddOrderAsync(
                    customerId,
                    model.DeliveryProvince,
                    model.DeliveryAddress
                );

                if (orderId <= 0)
                {
                    ModelState.AddModelError("", "Đặt hàng thất bại.");
                    ViewBag.Cart = cart;
                    ViewBag.Total = cart.Sum(c => c.Quantity * c.SalePrice);
                    return View(model);
                }

                foreach (var item in cart)
                {
                    var detail = new OrderDetail()
                    {
                        OrderID = orderId,
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        SalePrice = item.SalePrice
                    };

                    await SalesDataService.AddDetailAsync(detail);
                }

                ShoppingCartService.ClearCart();

                TempData["Message"] = "Đặt hàng thành công!";
                return RedirectToAction("Detail", new { id = orderId });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                ModelState.AddModelError("", "Lỗi khi đặt hàng.");

                ViewBag.Cart = cart;
                ViewBag.Total = cart.Sum(c => c.Quantity * c.SalePrice);
                return View(model);
            }
        }

        // ==================== HISTORY ====================

        [HttpGet]
        public IActionResult History()
        {
            try
            {
                var input = ApplicationContext.GetSessionData<CustomerOrderSearchInput>(ORDER_SEARCH_KEY)
                            ?? new CustomerOrderSearchInput { Page = 1, PageSize = 10 };

                ViewBag.Input = input;
                return View(input);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                ModelState.AddModelError("", "Không thể tải lịch sử đơn hàng.");
                return View(new CustomerOrderSearchInput());
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchHistory(CustomerOrderSearchInput input)
        {
            try
            {
                // 👉 LẤY USER ĐANG LOGIN
                var userData = User.GetUserData();
                if (userData == null)
                    return PartialView("_OrderList", new List<OrderViewInfo>());

                int customerId = int.Parse(userData.UserId!);

                // Fix page
                input.Page = input.Page < 1 ? 1 : input.Page;
                input.PageSize = input.PageSize < 1 ? 10 : input.PageSize;

                // 👉 QUAN TRỌNG: GÁN CUSTOMER ID
                var searchInput = new OrderSearchInput()
                {
                    Page = input.Page,
                    PageSize = input.PageSize,
                    Status = input.Status,
                    DateFrom = input.DateFrom,
                    DateTo = input.DateTo,

                    // 🔥 FIX Ở ĐÂY
                    CustomerID = customerId
                };

                var result = await SalesDataService.ListOrdersAsync(searchInput);

                return PartialView("_OrderList", result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return PartialView("_OrderList", new List<OrderViewInfo>());
            }
        }

        // ==================== DETAIL ====================

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var userData = User.GetUserData();
                if (userData == null) return RedirectToAction("Login", "Account");

                int customerId = int.Parse(userData.UserId!);
                var order = await SalesDataService.GetOrderAsync(id);

                if (order == null || order.CustomerID != customerId)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng.";
                    return RedirectToAction("History");
                }

                var details = await SalesDataService.ListDetailsAsync(id);
                ViewBag.Details = details;

                return View(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                TempData["Error"] = "Không thể xem chi tiết đơn hàng.";
                return RedirectToAction("History");
            }
        }

        // ==================== CANCEL ====================

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var userData = User.GetUserData();
                if (userData == null)
                {
                    ModelState.AddModelError("", "Chưa đăng nhập.");
                    return Json(new { success = false, message = "Chưa đăng nhập." });
                }

                int customerId = int.Parse(userData.UserId!);
                var order = await SalesDataService.GetOrderAsync(id);

                if (order == null || order.CustomerID != customerId)
                {
                    ModelState.AddModelError("", "Không tìm thấy đơn hàng.");
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
                }

                if (order.Status != OrderStatusEnum.New)
                {
                    ModelState.AddModelError("", "Không thể hủy.");
                    return Json(new
                    {
                        success = false,
                        message = "Chỉ hủy đơn ở trạng thái chờ."
                    });
                }

                bool result = await SalesDataService.CancelOrderAsync(id);

                return Json(new
                {
                    success = result,
                    message = result ? "Đã hủy đơn." : "Hủy thất bại."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                ModelState.AddModelError("", "Lỗi khi hủy đơn.");

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
    }
}