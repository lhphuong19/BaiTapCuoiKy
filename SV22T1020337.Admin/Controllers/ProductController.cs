using Microsoft.AspNetCore.Mvc;
using SV22T1020337.BusinessLayers;
using SV22T1020337.Models.Catalog;
using SV22T1020337.Models.Common;
using SV22T1020337.Models.Partner;
using System.Threading.Tasks;

namespace SV22T1020337.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng quản lý dữ liệu liên quan đến mặt hàng
    /// </summary>
    public class ProductController : Controller
    {
        private const string PRODUCT_SEARCH = "ProductSearchInput";
        /// <summary>
        /// Tìm kiếm và hiển thị danh sách sản phẩm
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH);

            if (input == null)
            {
                input = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = "",
                    CategoryID = 0,
                    SupplierID = 0,
                    MinPrice = 0,
                    MaxPrice = 0
                };
            }
            ViewBag.Suppliers = await PartnerDataService.ListSuppliersAsync(new PaginationSearchInput());
            ViewBag.Categories = await CatalogDataService.ListCategoriesAsync(new PaginationSearchInput());
            return View(input);
        }

        public async Task<IActionResult> Search(ProductSearchInput input)
        {
            var result = await CatalogDataService.ListProductsAsync(input);
            ApplicationContext.SetSessionData(PRODUCT_SEARCH, input);
            return View(result);
        }

        /// <summary>
        /// Tạo mới sản phẩm
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Thêm sản phẩm";
            var model = new Product()
            {
                ProductID = 0
            };
            return View("Edit", model);
        }

        /// <summary>
        /// Chỉnh sửa thông tin sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm cần thay dổi thông tin</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id)
        {
            //var product = await CatalogDataService.GetProductAsync(id);
            ViewBag.Photos = await CatalogDataService.ListPhotosAsync(id);
            ViewBag.Attributes = await CatalogDataService.ListAttributesAsync(id);
            var model = await CatalogDataService.GetProductAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }

        /// <summary>
        /// Xóa một sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm cần xóa</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int id)
        {
            if (Request.Method == "POST")
            {
                await CatalogDataService.DeleteProductAsync(id);
                return RedirectToAction("Index");
            }


            //GET
            var model = await CatalogDataService.GetProductAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            ViewBag.CanDelete = !await CatalogDataService.IsUsedProductAsync(id);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Product data, IFormFile? uploadPhoto) //Binding dữ liệu
        {
            try
            {
                ViewBag.Title = data.ProductID == 0 ? "Bổ sung mặt hàng" : "Cập nhật thông tin mặt hàng";

                //Kiểm tra tính đúng của dữ liệu

                //Sử dụng ModelState để lưu thông tin lỗi và chuyển thông báo lỗi ra View
                //Giả thiết: Yêu cầu phải nhập tên, email và tỉnh thành

                if (string.IsNullOrWhiteSpace(data.ProductName))
                    ModelState.AddModelError(nameof(data.ProductName), "Vui lòng nhập tên mặt hàng");

                if (string.IsNullOrWhiteSpace(data.Unit))
                    ModelState.AddModelError(nameof(data.Unit), "Vui lòng nhập đơn vị");

                if (data.Price < 0)
                    ModelState.AddModelError(nameof(data.Price), "Vui lòng nhập giá tiền");

                //Nếu dữ liệu không hợp lệ thì trả lại cho view để nhập lại
                if (!ModelState.IsValid)
                    return View("Edit", data);

                //Xử lý upload ảnh
                if (uploadPhoto != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(uploadPhoto.FileName)}";
                    var filePath = Path.Combine(ApplicationContext.WWWRootPath, "images/products", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadPhoto.CopyToAsync(stream);
                    }
                    data.Photo = fileName;
                };

                //Lưu dữ liệu vào CSDL
                if (data.ProductID == 0)
                    await CatalogDataService.AddProductAsync(data);
                else
                    await CatalogDataService.UpdateProductAsync(data);

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

        // ================== ATTRIBUTES ==================

        /// <summary>
        /// Hiển thị danh sách thuộc tính của mặt hàng
        /// </summary>
        /// <param name="id">Mã mặt hàng cần hiển thị thuộc tính</param>
        /// <returns></returns>
        public async Task<IActionResult> ListAttributes(int id, PaginationSearchInput input)
        {
            var result = await CatalogDataService.ListAttributesAsync(id);

            return View(result);
        }

        /// <summary>
        /// Bổ sung thuộc tính mới cho sản phẩm
        /// </summary>
        /// <param name="id">Mã mặt hàng cần bổ sung thuộc tính</param>
        /// <returns></returns>
        public IActionResult CreateAttribute(int id)
        {
            ViewBag.Title = "Thêm thuộc tính sản phẩm";
            return View("EditAttribute");
        }

        /// <summary>
        /// Cập nhật thuộc tính của sản phẩm
        /// </summary>
        /// <param name="id">mã sản phẩm có thuộc tính cần thay đổi</param>
        /// <param name="attributeId">Mã thuộc tính cần cập nhật</param>
        /// <returns></returns>
        public IActionResult EditAttribute(int id, int attributeId)
        {
            ViewBag.Title = "Chỉnh sửa thuộc tính sản phẩm";
            return View();
        }

        /// <summary>
        /// Xóa thuộc tính sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm có thuộc tính cần xóa</param>
        /// <param name="attributeId">Mã sản phẩm cần xóa</param>
        /// <returns></returns>
        public IActionResult DeleteAttribute(int id, int attributeId)
        {
            ViewBag.Title = "Xóa thuộc tính sản phẩm";
            return View();
            //return RedirectToAction("ListAttributes", new { id });
        }

        // ================== PHOTOS ==================

        /// <summary>
        /// Hiên thị danh sách ảnh của từng sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm cần hiển thị ảnh</param>
        /// <returns></returns>
        public async Task<IActionResult> ListPhotos(int id)
        {
            var result = await CatalogDataService.ListPhotosAsync(id);
            return View(result);
        }

        /// <summary>
        /// Bổ sung ảnh mới cho sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm cần bổ sung ảnh</param>
        /// <returns></returns>
        public IActionResult CreatePhoto(int id)
        {
            ViewBag.Title = "Thêm hình ảnh sản phẩm";
            return View("EditPhoto");
        }

        /// <summary>
        /// Cập nhật ảnh của sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm cần cập nhật ảnh</param>
        /// <param name="photoId">Mã ảnh cần cập nhật</param>
        /// <returns></returns>
        public IActionResult EditPhoto(int id, int photoId)
        {
            ViewBag.Title = "Cập nhật hình ảnh sản phẩm";
            return View();
        }

        /// <summary>
        /// Xóa ảnh của sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm có ảnh cần xóa</param>
        /// <param name="photoId">Mã ảnh cần xóa</param>
        /// <returns></returns>
        public IActionResult DeletePhoto(int id, int photoId)
        {
            ViewBag.Title = "Xóa hình ảnh sản phẩm";
            return View();
            //RedirectToAction("ListPhotos", new { id });
        }
    }
}