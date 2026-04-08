using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020337.Models.Common;
using SV22T1020337.Models.Partner;
using System.Threading.Tasks;

namespace SV22T1020337.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng quản lý dữ liệu liên quan đến người vận chuyển
    /// </summary>
    [Authorize]
    public class ShipperController : Controller
    {
        private const string SHIPPER_SEARCH = "ShipperSearchInput";
        /// <summary>
        /// Tìm kiếm và hiển thị danh sách người vận chuyển
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            try
            {
                var input = ApplicationContext.GetSessionData<PaginationSearchInput>(SHIPPER_SEARCH);
                if (input == null)
                    input = new PaginationSearchInput()
                    {
                        Page = 1,
                        PageSize = ApplicationContext.PageSize,
                        SearchValue = ""
                    };
                return View(input);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            try
            {
                var result = await PartnerDataService.ListShippersAsync(input);
                ApplicationContext.SetSessionData(SHIPPER_SEARCH, input);
                return View(result);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Bổ sung người vận chuyển mới
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            try
            {
                ViewBag.Title = "Bổ sung vận chuyển";
                var model = new Shipper()
                {
                    ShipperID = 0
                };
                return View("Edit", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Cập nhật thông tin người vận chuyển
        /// </summary>
        /// <param name="id">Mã người vận chuyển cần cập nhật thông tin</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                ViewBag.Title = "Cập nhật thông tin vận chuyển";
                var model = await PartnerDataService.GetShipperAsync(id);
                if (model == null)
                    return RedirectToAction("Index");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Xóa một người vận chuyển
        /// </summary>
        /// <param name="id">Mã người vận chuyển cần xóa</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                ViewBag.Title = "Xóa vận chuyển";
                if (Request.Method == "POST")
                {
                    await PartnerDataService.DeleteShipperAsync(id);
                    return RedirectToAction("Index");
                }


                //GET
                var model = await PartnerDataService.GetShipperAsync(id);
                if (model == null)
                    return RedirectToAction("Index");

                ViewBag.CanDelete = !await PartnerDataService.IsUsedShipperAsync(id);

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Shipper data) //Binding dữ liệu
        {
            try
            {
                ViewBag.Title = data.ShipperID == 0 ? "Bổ sung người giao hàng" : "Cập nhật thông tin người giao hàng";

                //Kiểm tra tính đúng của dữ liệu

                //Sử dụng ModelState để lưu thông tin lỗi và chuyển thông báo lỗi ra View
                //Giả thiết: Yêu cầu phải nhập tên, email và tỉnh thành

                if (string.IsNullOrWhiteSpace(data.ShipperName))
                    ModelState.AddModelError(nameof(data.ShipperName), "Vui lòng nhập tên người giao hàng");

                if (string.IsNullOrWhiteSpace(data.Phone))
                    ModelState.AddModelError(nameof(data.Phone), "Không được để trống số điện thoại");

                //Nếu dữ liệu không hợp lệ thì trả lại cho view để nhập lại
                if (!ModelState.IsValid)
                    return View("Edit", data);

                //Lưu dữ liệu vào CSDL
                if (data.ShipperID == 0)
                    await PartnerDataService.AddShipperAsync(data);
                else
                    await PartnerDataService.UpdateShipperAsync(data);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                //Ghi log lỗi với các thông tin nằm trong exception
                //ex.Message
                //ex.StackTrace
                ModelState.AddModelError("error", "Hệ thống tạm thời đang bận, vui lòng thử lại sau vài ngày");
                return View("Edit", data);
            }

        }

    }
}
