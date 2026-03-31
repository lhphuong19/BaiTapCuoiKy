using Microsoft.AspNetCore.Mvc;

namespace SV22T1020574.Admin.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhà mặt hàng";
            return View("Edit");
        }
        public IActionResult Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin mặt hàng";
            return View();
        }
        public IActionResult Delete(int id)
        {
            return View();
        }
        public IActionResult ListAttributes(int id)
        {
            return View();
        }
        public IActionResult CreateAttributes(int id)
        {
            ViewBag.Title = "Tạo mới thuộc tính";
            return View("EditAttribute");
        }
        public IActionResult EditAttribute(int id)
        {
            ViewBag.Title = "Chỉnh sửa thuộc tính";
            return View();
        }
        public IActionResult ListPhotos(int id)
        {
            return View();
        }
        public IActionResult DeleteAttribute(int id, int attributeId)
        {
            return View();
        }
        public IActionResult CreatePhoto(int id)
        {
            ViewBag.Title = "Tạo mới hình ảnh";
            return View("EditPhoto");
        }
        public IActionResult EditPhoto(int id)
        {
            ViewBag.Title = "Chỉnh sửa hình ảnh";
            return View();
        }
        public IActionResult DeletePhoto(int id, int attributeId)
        {
            return View();
        }

    }
}