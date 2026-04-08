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
            // Lưu hoặc lấy bộ lọc từ session
            if (filter == null || (filter.CategoryID == 0 && string.IsNullOrEmpty(filter.SearchValue)
                && filter.MinPrice == 0 && filter.MaxPrice == 0))
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

        // ==================== TÌM KIẾM (AJAX partial) ====================

        public async Task<IActionResult> Search(ProductFilterInputModel filter)
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

        // ==================== CHI TIẾT SẢN PHẨM ====================

        public async Task<IActionResult> Detail(int id)
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
    }
}
