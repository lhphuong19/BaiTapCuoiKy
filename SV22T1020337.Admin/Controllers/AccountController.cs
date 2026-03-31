using Microsoft.AspNetCore.Mvc;

namespace SV22T1020337.Admin.Controllers
{
    /// <summary>
    /// CUng cấp các chức năng quản lý liên quan đến tài khoản người dùng
    /// </summary>
    public class AccountController : Controller
    {
        /// <summary>
        /// Đăng nhập tài khoản người dùng
        /// </summary>
        /// <returns></returns>
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Đăng xuất tài khoản người dùng
        /// </summary>
        /// <returns></returns>
        public IActionResult Logout()
        {
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Thay đỏi mật khẩu cho tài khoản đang đăng nhập
        /// </summary>
        /// <returns></returns>
        public IActionResult ChangePassword()
        {
            return View();
        }
    }
}
