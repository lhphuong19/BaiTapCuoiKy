using Microsoft.AspNetCore.Mvc;
using SV22T1020337.BusinessLayers;
using SV22T1020337.Models.Partner;
using SV22T1020337.Models.Sales;

namespace SV22T1020337.Admin.Controllers
{
    /// <summary>
    /// Giao diện nhập đầu vào tìm kiếm và hiển thị kết quả tìm kiếm đơn hàng
    /// </summary>
    public class OrderController : Controller
    {
        private const string ORDER_SEARCH = "OrderSearchInput";
        /// <summary>
        /// Hiển thị danh sách đơn hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
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

        /// <summary>
        /// Tìm kiếm, hiển thị danh sách đơn hàng
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Search(OrderSearchInput input)
        {
            var result = await SalesDataService.ListOrdersAsync(input);

            ApplicationContext.SetSessionData(ORDER_SEARCH, input);
            return PartialView(result);
        }

        /// <summary>
        /// Tạo mới đơn hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Tạo đơn hàng";
            var model = new Order()
            {
                OrderID = 0
            };
            return View("Create", model);
        }

        /// <summary>
        /// Hiển thị chi tiết đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng cần xem chi tiết</param>
        /// <returns></returns>
        public IActionResult Detail(int id)
        {
            ViewBag.Title = "Chi tiết đơn hàng";
            return View();
        }

        // ================== CART ITEM ==================

        /// <summary>
        /// Cập nhật sản phẩm trong đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng cần cập nhật</param>
        /// <param name="productId">Mã sản phẩm cần cập nhật trong đơn hàng</param>
        /// <returns></returns>
        public IActionResult EditCartItem(int id, int productId)
        {
            ViewBag.Title = "Cập nhật mặt hàng trong đơn";
            return View();
        }

        /// <summary>
        /// Xóa sản phẩm có trong giỏ hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng cần xóa sản phẩm</param>
        /// <param name="productId">Mã sản phẩm cần xóa khỏi đơn hàng</param>
        /// <returns></returns>
        public IActionResult DeleteCartItem(int id, int productId)
        {
            ViewBag.Title = "Xóa mặt hàng khỏi đơn";
            return View();
        }

        /// <summary>
        /// Xóa toàn bộ sản trong giỏ hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult ClearCart()
        {
            ViewBag.Title = "Xóa toàn bộ giỏ hàng";
            return View();
        }

        // ================== TRẠNG THÁI ĐƠN HÀNG ==================

        /// <summary>
        /// Hiển thị trạng thái đươn hàng đã được duyệt
        /// </summary>
        /// <param name="id">Mã đơn hàng được duyệt</param>
        /// <returns></returns>
        public IActionResult Accept(int id)
        {
            ViewBag.Title = "Duyệt đơn hàng";
            return View();
        }

        /// <summary>
        /// Hiển thị đơn hàng đang được giao
        /// </summary>
        /// <param name="id">Mã đơn hàng đang được giao</param>
        /// <returns></returns>
        public IActionResult Shipping(int id)
        {
            ViewBag.Title = "Đang giao hàng";
            return View();
        }

        /// <summary>
        /// Hiển thị trạng thái đơn hàng đã giao hàng hoàn tất
        /// </summary>
        /// <param name="id">Mã đơn hàng đã được giao hoàn tất</param>
        /// <returns></returns>
        public IActionResult Finish(int id)
        {
            ViewBag.Title = "Hoàn tất đơn hàng";
            return View();
        }

        /// <summary>
        /// Hiển thị đơn hàng bị từ chối
        /// </summary>
        /// <param name="id">Mã đơn hàng bị từ chối</param>
        /// <returns></returns>
        public IActionResult Reject(int id)
        {
            ViewBag.Title = "Từ chối đơn hàng";
            return View();
        }

        /// <summary>
        /// Hiển thị đơn hàng bị hủy
        /// </summary>
        /// <param name="id">Mã đơn hàng đã bị hủy</param>
        /// <returns></returns>
        public IActionResult Cancel(int id)
        {
            ViewBag.Title = "Hủy đơn hàng";
            return View();
        }

        /// <summary>
        /// Hiển thị xóa đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng cần xóa</param>
        /// <returns></returns>
        public IActionResult Delete(int id)
        {
            ViewBag.Title = "Xóa đơn hàng";
            return View();
        }
    }
}