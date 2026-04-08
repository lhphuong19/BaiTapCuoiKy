using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020337.BusinessLayers;
using SV22T1020337.Models.Partner;
using SV22T1020337.Models.Security;
using SV22T1020337.Shop.Models;

namespace SV22T1020337.Shop.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        // ==================== ĐĂNG NHẬP ====================

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            try
            {
                if (User.Identity?.IsAuthenticated == true)
                    return RedirectToAction("Index", "Catalog");

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                ModelState.AddModelError("", "Không thể mở trang đăng nhập.");
                return View();
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            try
            {
                ViewBag.Email = model.Email;

                if (!ModelState.IsValid)
                    return View(model);

                string hashedPassword = CryptHelper.HashMD5(model.Password);
                var userAccount = await SecurityDataService.AuthorizeCustomerAsync(model.Email, hashedPassword);

                if (userAccount == null)
                {
                    ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                    return View(model);
                }

                var userData = new WebUserData()
                {
                    UserId = userAccount.UserId,
                    UserName = userAccount.UserName,
                    DisplayName = userAccount.DisplayName,
                    Email = userAccount.Email,
                    Photo = userAccount.Photo,
                    Roles = new List<string> { "customer" }
                };

                var principal = userData.CreatePrincipal();
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Catalog");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                ModelState.AddModelError("", "Đã xảy ra lỗi khi đăng nhập.");
                return View(model);
            }
        }

        // ==================== ĐĂNG XUẤT ====================

        public async Task<IActionResult> Logout()
        {
            try
            {
                HttpContext.Session.Clear();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                ModelState.AddModelError("", "Không thể đăng xuất.");
                return RedirectToAction("Index", "Catalog");
            }
        }

        // ==================== ĐĂNG KÝ ====================

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            try
            {
                if (User.Identity?.IsAuthenticated == true)
                    return RedirectToAction("Index", "Catalog");

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                ModelState.AddModelError("", "Không thể mở trang đăng ký.");
                return View();
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var customer = new Customer()
                {
                    CustomerName = model.CustomerName,
                    ContactName = model.ContactName,
                    Province = model.Province,
                    Address = model.Address,
                    Phone = model.Phone,
                    Email = model.Email,
                    IsLocked = false
                };

                int newId = await PartnerDataService.AddCustomerAsync(customer);
                if (newId <= 0)
                {
                    ModelState.AddModelError("", "Email đã tồn tại hoặc dữ liệu không hợp lệ.");
                    return View(model);
                }

                string hashedPassword = CryptHelper.HashMD5(model.Password);
                await SecurityDataService.SetCustomerPasswordAsync(model.Email, hashedPassword);

                TempData["Message"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                ModelState.AddModelError("", "Đã xảy ra lỗi khi đăng ký.");
                return View(model);
            }
        }

        // ==================== PROFILE ====================

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var userData = User.GetUserData();
                if (userData == null) return RedirectToAction("Login");

                int customerId = int.Parse(userData.UserId!);
                var customer = await PartnerDataService.GetCustomerAsync(customerId);
                if (customer == null) return RedirectToAction("Login");

                var model = new ProfileModel()
                {
                    CustomerID = customer.CustomerID,
                    CustomerName = customer.CustomerName,
                    ContactName = customer.ContactName,
                    Province = customer.Province ?? "",
                    Address = customer.Address ?? "",
                    Phone = customer.Phone ?? "",
                    Email = customer.Email
                };

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                ModelState.AddModelError("", "Không thể tải thông tin cá nhân.");
                return RedirectToAction("Index", "Catalog");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Profile(ProfileModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var customer = new Customer()
                {
                    CustomerID = model.CustomerID,
                    CustomerName = model.CustomerName,
                    ContactName = model.ContactName,
                    Province = model.Province,
                    Address = model.Address,
                    Phone = model.Phone,
                    Email = model.Email
                };

                bool result = await PartnerDataService.UpdateCustomerAsync(customer);
                if (!result)
                {
                    ModelState.AddModelError("", "Cập nhật thất bại.");
                    return View(model);
                }

                TempData["Message"] = "Cập nhật thành công!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                ModelState.AddModelError("", "Lỗi khi cập nhật thông tin.");
                return View(model);
            }
        }

        // ==================== ĐỔI MẬT KHẨU ====================

        [HttpGet]
        public IActionResult ChangePassword()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                ModelState.AddModelError("", "Không thể mở trang đổi mật khẩu.");
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            try
            {
                // Validate
                if (string.IsNullOrEmpty(oldPassword))
                    ModelState.AddModelError("OldPassword", "Nhập mật khẩu cũ");

                if (string.IsNullOrEmpty(newPassword))
                    ModelState.AddModelError("NewPassword", "Nhập mật khẩu mới");

                if (string.IsNullOrEmpty(confirmPassword))
                    ModelState.AddModelError("ConfirmPassword", "Nhập lại mật khẩu");

                if (newPassword != confirmPassword)
                    ModelState.AddModelError("ConfirmPassword", "Xác nhận mật khẩu không trùng");

                if (oldPassword == newPassword)
                    ModelState.AddModelError("NewPassword", "Mật khẩu mới không được trùng");

                if (!ModelState.IsValid)
                    return View();

                // Lấy user
                var userData = User.GetUserData();
                if (userData == null) return RedirectToAction("Login");

                string email = userData.Email!;

                // Hash
                string oldPass = CryptHelper.HashMD5(oldPassword);
                string newPass = CryptHelper.HashMD5(newPassword);

                var account = await SecurityDataService.AuthorizeCustomerAsync(email, oldPass);

                if (account == null)
                {
                    ModelState.AddModelError("OldPassword", "Mật khẩu cũ không đúng");
                    return View();
                }

                bool result = await SecurityDataService.ChangePasswordAsync(email, newPass);

                if (!result)
                {
                    ModelState.AddModelError("", "Không thể đổi mật khẩu");
                    return View();
                }

                // 👉 THÀNH CÔNG
                ViewBag.Success = "Đổi mật khẩu thành công!";

                // 👉 reset form (rất quan trọng)
                ModelState.Clear();

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                ModelState.AddModelError("", "Hệ thống lỗi");
                return View();
            }
        }

        // ==================== ACCESS DENIED ====================

        public IActionResult AccessDenied()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return Content("Access Denied");
            }
        }
    }
}