using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020337.BusinessLayers;
using SV22T1020337.Models.Common;
using SV22T1020337.Models.Partner;
using System.Threading.Tasks;

namespace SV22T1020337.Admin.Controllers
{
    /// <summary>
    /// cung cấp các chức năng quản lý dữ liệu liên quan đến nhà cung cấp
    /// </summary>
    [Authorize]
    public class SupplierController : Controller
    {
        private const string SUPPLIER_SEARCH = "SupplierSearchInput";
        /// <summary>
        /// Tìm kiếm và hiển thị danh sách nhà cung cấp
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            try
            {
                var input = ApplicationContext.GetSessionData<PaginationSearchInput>(SUPPLIER_SEARCH);
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
                var result = await PartnerDataService.ListSuppliersAsync(input);
                ApplicationContext.SetSessionData(SUPPLIER_SEARCH, input);
                return View(result);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Bổ sung nhà cung cấp mới
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            try
            {
                ViewBag.Title = "Bổ sung nhà cung cấp";
                var model = new Supplier()
                {
                    SupplierID = 0
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
        /// Cập nhật thông tin nhà cung cấp
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần cập nhật thông tin</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                ViewBag.Title = "Cập nhật thông tin nhà cung cấp";
                var model = await PartnerDataService.GetSupplierAsync(id);
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
        /// Xóa một nhà cung cấp
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần xóa</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (Request.Method == "POST")
                {
                    await PartnerDataService.DeleteSupplierAsync(id);
                    return RedirectToAction("Index");
                }


                //GET
                var model = await PartnerDataService.GetSupplierAsync(id);
                if (model == null)
                    return RedirectToAction("Index");

                ViewBag.CanDelete = !await PartnerDataService.IsUsedSupplierAsync(id);

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Supplier data) //Binding dữ liệu
        {
            try
            {
                ViewBag.Title = data.SupplierID == 0 ? "Bổ sung Nhà cung cấp" : "Cập nhật thông tin Nhà cung cấp";

                //Kiểm tra tính đúng của dữ liệu

                //Sử dụng ModelState để lưu thông tin lỗi và chuyển thông báo lỗi ra View
                //Giả thiết: Yêu cầu phải nhập tên, email và tỉnh thành

                if (string.IsNullOrWhiteSpace(data.SupplierName))
                    ModelState.AddModelError(nameof(data.SupplierName), "Vui lòng nhập tên nha cung cấp");

                if (string.IsNullOrWhiteSpace(data.Phone))
                    ModelState.AddModelError(nameof(data.Phone), "Vui lòng nhập số điện thoại");

                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError(nameof(data.Email), "Email không được để trống");
                //else if (!await PartnerDataService.ValidatelCustomerEmailAsync(data.Email, data.SupplierID))
                //    ModelState.AddModelError(nameof(data.Email), "Email đã được khách hàng khác dùng");

                if (string.IsNullOrWhiteSpace(data.Province))
                    ModelState.AddModelError(nameof(data.Province), "Nhập Tỉnh/thành");

                //Nếu dữ liệu không hợp lệ thì trả lại cho view để nhập lại
                if (!ModelState.IsValid)
                    return View("Edit", data);

                //(TUỲ CHỌN) Hiệu chỉnh dữ liệu theo quy định của hệ thống
                if (string.IsNullOrWhiteSpace(data.ContactName)) data.ContactName = data.SupplierName;
                if (string.IsNullOrEmpty(data.Address)) data.Address = "";
                if (string.IsNullOrEmpty(data.Email)) data.Email = "";

                //Lưu dữ liệu vào CSDL
                if (data.SupplierID == 0)
                    await PartnerDataService.AddSupplierAsync(data);
                else
                    await PartnerDataService.UpdateSupplierAsync(data);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("error", "Hệ thống tạm thời đang bận, vui lòng thử lại sau vài ngày");
                return View("Edit", data);
            }
        }
    }
}