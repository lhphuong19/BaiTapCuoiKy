using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020337.BusinessLayers;
using SV22T1020337.Models.Security;
using System.Threading.Tasks;

namespace SV22T1020337.Admin.Controllers
{
    /// <summary>
    /// CUng cấp các chức năng quản lý liên quan đến tài khoản người dùng
    /// </summary>
    [Authorize]
    public class AccountController : Controller
    {
        /// <summary>
        /// Đăng nhập tài khoản người dùng
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            try
            {
                ViewBag.UserName = username;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError("Error", "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");
                    return View();
                }

                // Mã hoá MD5 mật khẩu
                string hashedPassword = CryptHelper.HashMD5(password);

                // Kiểm tra username và hashedPassword với cơ sở dữ liệu
                var userAccount = await SecurityDataService.AuthorizeAsync(username, hashedPassword);
                if (userAccount == null)
                {
                    ModelState.AddModelError("Error", "Tên đăng nhập hoặc mật khẩu không đúng.");
                    return View();
                }

                // Chặn khách hàng đăng nhập vào trang Admin
                if (userAccount.RoleNames == "Customer")
                {
                    ModelState.AddModelError("Error", "Tài khoản không có quyền truy cập hệ thống quản trị.");
                    return View();
                }

                // Xử lý đăng nhập thành công
                // 1. Chuẩn bị thông tin sẽ ghi trong principal (ClaimPrincipal)
                var userData = new WebUserData()
                {
                    UserId = userAccount.UserId,
                    UserName = userAccount.UserName,
                    DisplayName = userAccount.DisplayName,
                    Email = userAccount.Email,
                    Photo = userAccount.Photo,
                    Roles = userAccount.RoleNames.Split(',').ToList()
                };

                // 2. Tạo chứng nhận (claimsprincipal) cho người dùng
                var principal = userData.CreatePrincipal();

                // 3. Cấp chứng nhận cho người dùng
                await HttpContext.SignInAsync(principal);

                return RedirectToAction("Index", "Home");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return View();
            }
        }

        /// <summary>
        /// Đăng xuất tài khoản người dùng
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            try
            {
                HttpContext.Session.Clear();
                await HttpContext.SignOutAsync();
                return RedirectToAction("Login");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Login");
            }
            
        }

        /// <summary>
        /// Thay đỏi mật khẩu cho tài khoản đang đăng nhập
        /// </summary>
        /// <returns></returns>
        public IActionResult ChangePassword()
        {
            try
            {
                return View();
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Login");
            }
        }

        /// <summary>
        /// Xử lý đổi mật khẩu
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(oldPassword))
                    ModelState.AddModelError("oldPassword", "Nhập mật khẩu cũ");

                if (string.IsNullOrEmpty(newPassword))
                    ModelState.AddModelError("newPassword", "Nhập mật khẩu mới");

                if (string.IsNullOrEmpty(confirmPassword))
                    ModelState.AddModelError("confirmPassword", "Nhập lại mật khẩu");

                //Check logic
                if (newPassword != confirmPassword)
                    ModelState.AddModelError("confirmPassword", "Nhập lại mật khẩu không trùng");

                if (oldPassword == newPassword)
                    ModelState.AddModelError("newPassword", "Mật khẩu mới không được trùng mật khẩu cũ");

                if (!ModelState.IsValid)
                    return View();

                //Lấy username
                var userName = User.GetUserData()?.UserName;
                if (string.IsNullOrEmpty(userName))
                    return RedirectToAction("Login");

                //Hash mật khẩu
                string oldPassMD5 = CryptHelper.HashMD5(oldPassword);
                string newPassMD5 = CryptHelper.HashMD5(newPassword);

                var emp = await SecurityDataService.AuthorizeAsync(userName, oldPassMD5);

                if (emp == null)
                {
                    ModelState.AddModelError("oldPassword", "Mật khẩu cũ không đúng");
                    return View();
                }

                // Đổi mật khẩu
                bool result = await SecurityDataService.ChangePasswordAsync(userName, newPassMD5);

                if (!result)
                {
                    ModelState.AddModelError("", "Không thể đổi mật khẩu");
                    return View();
                }

                ViewBag.Success = "Đổi mật khẩu thành công";
                return View();
            }
            catch (Exception ex)
            {
                // Có thể log lỗi ở đây nếu cần
                // ex.Message, ex.StackTrace

                ModelState.AddModelError("", "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return View();
            }
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
