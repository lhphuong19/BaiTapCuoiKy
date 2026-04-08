using Microsoft.AspNetCore.Mvc;
using SV22T1020337.BusinessLayers;
using SV22T1020337.Models.Catalog;
using SV22T1020337.Models.Common;
using SV22T1020337.Shop.Models;

namespace SV22T1020337.Shop.Controllers
{
    public class CatalogController : Controller
    {
        private const string PRODUCT_SEARCH_KEY = "ShopProductSearch";

        // ==================== DANH MỤC SẢN PHẨM ====================

        public async Task<IActionResult> Index(ProductFilterInputModel? filter)
        {
            try
            {
                // Chỉ lấy từ session khi không có bất kỳ tham số nào được truyền vào
                if (filter == null || (!Request.Query.ContainsKey("SearchValue")
                    && !Request.Query.ContainsKey("CategoryID")
                    && !Request.Query.ContainsKey("MinPrice")
                    && !Request.Query.ContainsKey("MaxPrice")))
                {
                    var saved = ApplicationContext.GetSessionData<ProductFilterInputModel>(PRODUCT_SEARCH_KEY);
                    if (saved != null) filter = saved;
                }

                filter ??= new ProductFilterInputModel();
                filter.Page = filter.Page < 1 ? 1 : filter.Page;
                filter.PageSize = filter.PageSize < 1 ? 12 : filter.PageSize;

                ApplicationContext.SetSessionData(PRODUCT_SEARCH_KEY, filter);

                var input = new ProductSearchInput()
                {
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    SearchValue = filter.SearchValue ?? "",
                    CategoryID = filter.CategoryID,
                    MinPrice = filter.MinPrice,
                    MaxPrice = filter.MaxPrice
                };

                var products = await CatalogDataService.ListProductsAsync(input);
                var categories = await CatalogDataService.ListCategoriesAsync(
                    new PaginationSearchInput { Page = 1, PageSize = 0, SearchValue = "" });

                ViewBag.Filter = filter;
                ViewBag.Categories = categories.DataItems;

                return View(products);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                ModelState.AddModelError("", "Không thể tải danh sách sản phẩm.");

                ViewBag.Filter = new ProductFilterInputModel();
                ViewBag.Categories = new List<Category>();

                return View(new PagedResult<Product>());
            }
        }

        // ==================== TÌM KIẾM (AJAX partial) ====================

        public async Task<IActionResult> Search(ProductFilterInputModel filter)
        {
            try
            {
                filter.Page = filter.Page < 1 ? 1 : filter.Page;
                filter.PageSize = filter.PageSize < 1 ? 12 : filter.PageSize;

                ApplicationContext.SetSessionData(PRODUCT_SEARCH_KEY, filter);

                var input = new ProductSearchInput()
                {
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    SearchValue = filter.SearchValue ?? "",
                    CategoryID = filter.CategoryID,
                    MinPrice = filter.MinPrice,
                    MaxPrice = filter.MaxPrice
                };

                var products = await CatalogDataService.ListProductsAsync(input);
                ViewBag.Filter = filter;

                return PartialView("_ProductList", products);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return PartialView("_ProductList", new PagedResult<Product>());
            }
        }

        // ==================== CHI TIẾT SẢN PHẨM ====================

        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var product = await CatalogDataService.GetProductAsync(id);
                if (product == null)
                    return RedirectToAction("Index");

                var photos = await CatalogDataService.ListPhotosAsync(id);
                var attributes = await CatalogDataService.ListAttributesAsync(id);

                ViewBag.Photos = photos;
                ViewBag.Attributes = attributes;

                return View(product);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                TempData["Error"] = "Không thể tải chi tiết sản phẩm.";
                return RedirectToAction("Index");
            }
        }
    }
}