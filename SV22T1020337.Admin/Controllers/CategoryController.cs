using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020337.BusinessLayers;
using SV22T1020337.Models.Catalog;
using SV22T1020337.Models.Common;
using SV22T1020337.Models.Partner;
using System.Threading.Tasks;

namespace SV22T1020337.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng quản lý dữ liệu liên quan đến loại hàng
    /// </summary>
    [Authorize]
    public class CategoryController : Controller
    {
        private const string CATEGORY_SEARCH = "CategorySearchInput";
        /// <summary>
        /// Tìm kiếm và hiển thị danh sách loại hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            try
            {
                var input = ApplicationContext.GetSessionData<PaginationSearchInput>(CATEGORY_SEARCH);
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
                var result = await CatalogDataService.ListCategoriesAsync(input);
                ApplicationContext.SetSessionData(CATEGORY_SEARCH, input);
                return View(result);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Bổ sung các loại hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            try
            {
                ViewBag.Title = "Bổ sung loại hàng";
                var model = new Category()
                {
                    CategoryID = 0
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
        /// Cập nhật thông tin loại hàng
        /// </summary>
        /// <param name="id">Mã loại hàng cập nhật</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                ViewBag.Title = "Cập nhật loại hàng";
                var model = await CatalogDataService.GetCategoryAsync(id);
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
        /// Xóa thông tin loại hàng
        /// </summary>
        /// <param name="id">Mã loại hàng cần xóa</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                ViewBag.Title = "Xóa loại hàng";
                if (Request.Method == "POST")
                {
                    await CatalogDataService.DeleteCategoryAsync(id);
                    return RedirectToAction("Index");
                }


                //GET
                var model = await CatalogDataService.GetCategoryAsync(id);
                if (model == null)
                    return RedirectToAction("Index");

                ViewBag.CanDelete = !await CatalogDataService.IsUsedCategoryAsync(id);

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Category data) //Binding dữ liệu
        {
            try
            {
                ViewBag.Title = data.CategoryID == 0 ? "Bổ sung Loại hàng" : "Cập nhật thông tin loại hàng";

                //Kiểm tra tính đúng của dữ liệu

                //Sử dụng ModelState để lưu thông tin lỗi và chuyển thông báo lỗi ra View
                //Giả thiết: Yêu cầu phải nhập tên, email và tỉnh thành

                if (string.IsNullOrWhiteSpace(data.CategoryName))
                    ModelState.AddModelError(nameof(data.CategoryName), "Vui lòng nhập tên loại hàng");

                //Nếu dữ liệu không hợp lệ thì trả lại cho view để nhập lại
                if (!ModelState.IsValid)
                    return View("Edit", data);

                //(TUỲ CHỌN) Hiệu chỉnh dữ liệu theo quy định của hệ thống
                if (string.IsNullOrWhiteSpace(data.Description)) data.Description = "";

                //Lưu dữ liệu vào CSDL
                if (data.CategoryID == 0)
                    await CatalogDataService.AddCategoryAsync(data);
                else
                    await CatalogDataService.UpdateCategoryAsync(data);

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
