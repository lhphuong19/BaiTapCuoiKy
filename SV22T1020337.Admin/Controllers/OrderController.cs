using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020337.BusinessLayers;
using SV22T1020337.Models.Catalog;
using SV22T1020337.Models.Common;
using SV22T1020337.Models.Partner;
using SV22T1020337.Models.Sales;
using System.Threading.Tasks;

namespace SV22T1020337.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng quản lý đơn hàng
    /// </summary>
    [Authorize(Roles = $"{WebUserRoles.Administrator},{WebUserRoles.Sales}")]
    public class OrderController : Controller
    {
        private const string PRODUCT_SEARCH = "SearchProductToSale";
        private const string ORDER_SEARCH = "OrderSearchInput";

        /// <summary>
        /// Hiển thị danh sách đơn hàng
        /// </summary>
        public IActionResult Index()
        {
            try
            {
                var input = ApplicationContext.GetSessionData<OrderSearchInput>(ORDER_SEARCH);
                if (input == null)
                {
                    input = new OrderSearchInput()
                    {
                        Page = 1,
                        PageSize = ApplicationContext.PageSize,
                        SearchValue = "",
                    };
                }
                return View(input);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Tìm kiếm và hiển thị danh sách đơn hàng (partial)
        /// </summary>
        public async Task<IActionResult> Search(OrderSearchInput input)
        {
            try
            {
                var result = await SalesDataService.ListOrdersAsync(input);
                ApplicationContext.SetSessionData(ORDER_SEARCH, input);
                return PartialView(result);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return PartialView(new SV22T1020337.Models.Common.PagedResult<OrderViewInfo>());
            }
        }

        /// <summary>
        /// Tạo mới đơn hàng — hiển thị màn hình chọn sản phẩm
        /// </summary>
        public async Task<IActionResult> Create()
        {
            try
            {
                var input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH);
                if (input == null)
                {
                    input = new ProductSearchInput()
                    {
                        Page = 1,
                        PageSize = ApplicationContext.PageSize,
                        SearchValue = "",
                    };
                }
                ViewBag.Customers = await PartnerDataService.ListCustomersAsync(new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = 1000,
                    SearchValue = ""
                });
                return View(input);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Tìm kiếm sản phẩm khi lập đơn hàng
        /// </summary>
        public async Task<IActionResult> SearchProduct(ProductSearchInput input)
        {
            try
            {
                var result = await CatalogDataService.ListProductsAsync(input);
                ApplicationContext.SetSessionData(PRODUCT_SEARCH, input);
                return PartialView(result);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return PartialView(new SV22T1020337.Models.Common.PagedResult<Product>());
            }
        }

        /// <summary>
        /// Hiển thị giỏ hàng hiện tại
        /// </summary>
        public IActionResult ShowCart()
        {
            try
            {
                var cart = ShoppingCartService.GetShoppingCart();
                return View(cart);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Create");
            }
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddCartItem(int productId, int quantity, decimal price)
        {
            try
            {
                if (quantity <= 0)
                    return Json(new ApiResult(0, "Số lượng không hợp lệ"));
                if (price <= 0)
                    return Json(new ApiResult(0, "Giá bán không hợp lệ"));

                var product = await CatalogDataService.GetProductAsync(productId);
                if (product == null)
                    return Json(new ApiResult(0, "Sản phẩm không tồn tại"));
                if (!product.IsSelling)
                    return Json(new ApiResult(0, "Sản phẩm ngừng bán"));

                ShoppingCartService.AddCartItem(new OrderDetailViewInfo()
                {
                    ProductID = productId,
                    Quantity = quantity,
                    SalePrice = price,
                    ProductName = product.ProductName,
                    Unit = product.Unit,
                    Photo = product.Photo ?? "nophoto.png"
                });
                return Json(new ApiResult(1));
            }
            catch (Exception ex)
            {
                return Json(new ApiResult(0, "Có lỗi xảy ra khi thêm vào giỏ hàng"));
            }
        }

        /// <summary>
        /// Hiển thị chi tiết đơn hàng
        /// </summary>
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var order = await SalesDataService.GetOrderAsync(id);
                if (order == null)
                    return RedirectToAction("Index");

                ViewBag.Title = "Chi tiết đơn hàng";
                ViewBag.Details = await SalesDataService.ListDetailsAsync(id);
                return View(order);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        // ================== CART ITEM ==================

        /// <summary>
        /// Hiển thị form cập nhật mặt hàng trong giỏ hàng
        /// </summary>
        public async Task<IActionResult> EditCartItem(int productId = 0, int orderId = 0)
        {
            try
            {
                // Thử lấy từ giỏ hàng tạm (session) trước
                var item = ShoppingCartService.GetCartItem(productId);

                // Nếu không có trong session (đơn hàng đã lưu vào DB), lấy từ DB
                if (item == null && orderId > 0)
                {
                    item = await SalesDataService.GetDetailAsync(orderId, productId);
                }

                if (item == null)
                    return Json(new ApiResult(0, "Không tìm thấy mặt hàng"));

                return PartialView(item);
            }
            catch (Exception ex)
            {
                return Json(new ApiResult(0, "Có lỗi xảy ra"));
            }
        }

        /// <summary>
        /// Cập nhật số lượng và giá mặt hàng trong giỏ
        /// </summary>
        [HttpPost]
        public IActionResult UpdateCartItem(int productId, int quantity, decimal salePrice)
        {
            try
            {
                if (quantity <= 0)
                    return Json(new ApiResult(0, "Số lượng không hợp lệ"));
                if (salePrice <= 0)
                    return Json(new ApiResult(0, "Giá bán không hợp lệ"));

                ShoppingCartService.UpdateCartItem(productId, quantity, salePrice);
                return Json(new ApiResult(1));
            }
            catch (Exception ex)
            {
                return Json(new ApiResult(0, "Có lỗi xảy ra khi cập nhật giỏ hàng"));
            }
        }

        /// <summary>
        /// Cập nhật số lượng và giá mặt hàng trong đơn hàng đã lưu vào DB
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateDetail(int orderId, int productId, int quantity, decimal salePrice)
        {
            try
            {
                if (quantity <= 0)
                    return Json(new ApiResult(0, "Số lượng không hợp lệ"));
                if (salePrice <= 0)
                    return Json(new ApiResult(0, "Giá bán không hợp lệ"));

                var data = new OrderDetail()
                {
                    OrderID = orderId,
                    ProductID = productId,
                    Quantity = quantity,
                    SalePrice = salePrice
                };

                bool result = await SalesDataService.UpdateDetailAsync(data);
                if (!result)
                    return Json(new ApiResult(0, "Không thể cập nhật. Đơn hàng có thể không còn ở trạng thái mới."));

                return Json(new ApiResult(1));
            }
            catch (Exception ex)
            {
                return Json(new ApiResult(0, "Có lỗi xảy ra khi cập nhật mặt hàng"));
            }
        }

        /// <summary>
        /// Xóa một mặt hàng khỏi giỏ hàng
        /// </summary>
        public async Task<IActionResult> DeleteCartItem(int productId = 0, int orderId = 0)
        {
            try
            {
                if (Request.Method == "POST")
                {
                    ShoppingCartService.RemoveCartItem(productId);
                    return Json(new ApiResult(1));
                }

                // Thử lấy từ session trước, nếu không có thì lấy từ DB
                var item = ShoppingCartService.GetCartItem(productId);
                if (item == null && orderId > 0)
                    item = await SalesDataService.GetDetailAsync(orderId, productId);

                if (item == null)
                    return Json(new ApiResult(0, "Không tìm thấy mặt hàng"));

                return PartialView(item);
            }
            catch (Exception ex)
            {
                return Json(new ApiResult(0, "Có lỗi xảy ra khi xóa khỏi giỏ hàng"));
            }
        }

        /// <summary>
        /// Xóa một mặt hàng khỏi đơn hàng đã lưu vào DB
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteDetail(int orderId, int productId)
        {
            try
            {
                bool result = await SalesDataService.DeleteDetailAsync(orderId, productId);
                if (!result)
                    return Json(new ApiResult(0, "Không thể xóa. Đơn hàng có thể không còn ở trạng thái mới."));

                return Json(new ApiResult(1));
            }
            catch (Exception ex)
            {
                return Json(new ApiResult(0, "Có lỗi xảy ra khi xóa mặt hàng"));
            }
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        public IActionResult ClearCart()
        {
            try
            {
                if (Request.Method == "POST")
                {
                    ShoppingCartService.ClearCart();
                    return Json(new ApiResult(1));
                }
                return PartialView();
            }
            catch (Exception ex)
            {
                return Json(new ApiResult(0, "Có lỗi xảy ra khi xóa giỏ hàng"));
            }
        }

        /// <summary>
        /// Tạo đơn hàng từ giỏ hàng hiện tại
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateOrder(int customerID = 0, string province = "", string address = "")
        {
            try
            {
                var cart = ShoppingCartService.GetShoppingCart();
                if (cart.Count == 0)
                    return Json(new ApiResult(0, "Giỏ hàng trống, không lập được đơn hàng"));

                if (string.IsNullOrWhiteSpace(address))
                    return Json(new ApiResult(0, "Vui lòng nhập địa chỉ giao hàng"));

                if (string.IsNullOrWhiteSpace(province))
                    return Json(new ApiResult(0, "Vui lòng chọn tỉnh/thành giao hàng"));

                if (customerID > 0)
                {
                    var customer = await PartnerDataService.GetCustomerAsync(customerID);
                    if (customer == null)
                        return Json(new ApiResult(0, "Khách hàng không tồn tại"));
                    if (customer.IsLocked)
                        return Json(new ApiResult(0, "Tài khoản khách hàng đã bị khóa"));
                }

                int orderID = await SalesDataService.AddOrderAsync(customerID, province, address);
                if (orderID <= 0)
                    return Json(new ApiResult(0, "Tạo đơn hàng thất bại, vui lòng thử lại"));

                foreach (var item in cart)
                {
                    await SalesDataService.AddDetailAsync(new OrderDetail()
                    {
                        OrderID = orderID,
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        SalePrice = item.SalePrice,
                    });
                }

                ShoppingCartService.ClearCart();
                return Json(new ApiResult(orderID));
            }
            catch (Exception ex)
            {
                return Json(new ApiResult(0, ex.Message));
            }
        }

        // ================== TRẠNG THÁI ĐƠN HÀNG ==================

        /// <summary>
        /// Duyệt đơn hàng (GET: xác nhận | POST: thực hiện)
        /// </summary>
        public async Task<IActionResult> Accept(int id)
        {
            try
            {
                var order = await SalesDataService.GetOrderAsync(id);
                if (order == null)
                    return RedirectToAction("Index");

                if (Request.Method == "POST")
                {
                    int employeeID = int.TryParse(User.GetUserData()?.UserId, out int eid) ? eid : 0;
                    bool result = await SalesDataService.AcceptOrderAsync(id, employeeID);
                    if (!result)
                    {
                        ModelState.AddModelError("", "Không thể duyệt đơn hàng. Đơn hàng có thể đã được xử lý trước đó.");
                        ViewBag.Title = "Duyệt đơn hàng";
                        return View(order);
                    }
                    return RedirectToAction("Detail", new { id });
                }

                ViewBag.Title = "Duyệt đơn hàng";
                return View(order);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Từ chối đơn hàng (GET: xác nhận | POST: thực hiện)
        /// </summary>
        public async Task<IActionResult> Reject(int id)
        {
            try
            {
                var order = await SalesDataService.GetOrderAsync(id);
                if (order == null)
                    return RedirectToAction("Index");

                if (Request.Method == "POST")
                {
                    int employeeID = int.TryParse(User.GetUserData()?.UserId, out int eid) ? eid : 0;
                    bool result = await SalesDataService.RejectOrderAsync(id, employeeID);
                    if (!result)
                    {
                        ModelState.AddModelError("", "Không thể từ chối đơn hàng. Đơn hàng có thể đã được xử lý trước đó.");
                        ViewBag.Title = "Từ chối đơn hàng";
                        return View(order);
                    }
                    return RedirectToAction("Detail", new { id });
                }

                ViewBag.Title = "Từ chối đơn hàng";
                return View(order);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Hủy đơn hàng (GET: xác nhận | POST: thực hiện)
        /// </summary>
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var order = await SalesDataService.GetOrderAsync(id);
                if (order == null)
                    return RedirectToAction("Index");

                if (Request.Method == "POST")
                {
                    bool result = await SalesDataService.CancelOrderAsync(id);
                    if (!result)
                    {
                        ModelState.AddModelError("", "Không thể hủy đơn hàng. Chỉ được hủy đơn hàng ở trạng thái Mới hoặc Đã duyệt.");
                        ViewBag.Title = "Hủy đơn hàng";
                        return View(order);
                    }
                    return RedirectToAction("Detail", new { id });
                }

                ViewBag.Title = "Hủy đơn hàng";
                return View(order);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Giao đơn hàng cho người vận chuyển (GET: chọn shipper | POST: thực hiện)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Shipping(int id)
        {
            try
            {
                var order = await SalesDataService.GetOrderAsync(id);
                if (order == null)
                    return RedirectToAction("Index");

                ViewBag.Title = "Giao hàng";
                ViewBag.Shippers = await PartnerDataService.ListShippersAsync(new PaginationSearchInput());

                return PartialView(order);
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Shipping(int id, int shipperID)
        {
            try
            {
                var order = await SalesDataService.GetOrderAsync(id);
                if (order == null)
                    return RedirectToAction("Index");

                // Chưa chọn shipper
                if (shipperID <= 0)
                {
                    ModelState.AddModelError("", "Vui lòng chọn người giao hàng");
                }
                else
                {
                    bool result = await SalesDataService.ShipOrderAsync(id, shipperID);

                    if (!result)
                    {
                        ModelState.AddModelError("", "Không thể giao đơn hàng. Đơn hàng phải ở trạng thái Đã duyệt.");
                    }
                    else
                    {
                        return RedirectToAction("Detail", new { id });
                    }
                }
                ViewBag.Title = "Giao hàng";
                ViewBag.Shippers = await PartnerDataService.ListShippersAsync(new PaginationSearchInput());

                return PartialView(order);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Hệ thống lỗi, vui lòng thử lại");

                ViewBag.Title = "Giao hàng";
                ViewBag.Shippers = await PartnerDataService.ListShippersAsync(new PaginationSearchInput());

                return View(await SalesDataService.GetOrderAsync(id));
            }
        }

        /// <summary>
        /// Hoàn tất đơn hàng (GET: xác nhận | POST: thực hiện)
        /// </summary>
        public async Task<IActionResult> Finish(int id)
        {
            try
            {
                var order = await SalesDataService.GetOrderAsync(id);
                if (order == null)
                    return RedirectToAction("Index");

                if (Request.Method == "POST")
                {
                    bool result = await SalesDataService.CompleteOrderAsync(id);
                    if (!result)
                    {
                        ModelState.AddModelError("", "Không thể hoàn tất đơn hàng. Đơn hàng phải đang ở trạng thái Đang giao.");
                        ViewBag.Title = "Hoàn tất đơn hàng";
                        return View(order);
                    }
                    return RedirectToAction("Detail", new { id });
                }

                ViewBag.Title = "Hoàn tất đơn hàng";
                return View(order);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Xóa đơn hàng (GET: xác nhận | POST: thực hiện)
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var order = await SalesDataService.GetOrderAsync(id);
                if (order == null)
                    return RedirectToAction("Index");

                if (Request.Method == "POST")
                {
                    bool result = await SalesDataService.DeleteOrderAsync(id);
                    if (!result)
                    {
                        ModelState.AddModelError("", "Không thể xóa đơn hàng. Chỉ được xóa đơn hàng ở trạng thái Mới.");
                        ViewBag.Title = "Xóa đơn hàng";
                        return View(order);
                    }
                    return RedirectToAction("Index");
                }

                ViewBag.Title = "Xóa đơn hàng";
                return View(order);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }
    }
}