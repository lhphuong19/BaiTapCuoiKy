using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020337.BusinessLayers;
using SV22T1020337.Models.Common;
using SV22T1020337.Models.HR;
using System.Threading.Tasks;

namespace SV22T1020337.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng quản lý dữ liệu liên quan đến Nhân viên
    /// </summary>
    [Authorize]
    public class EmployeeController : Controller
    {
        private const string EMPLOYEE_SEARCH = "EmployeeSearchInput";
        /// <summary>
        /// Tìm kiếm và hiển thị danh sách nhân viên
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            try
            {
                var input = ApplicationContext.GetSessionData<PaginationSearchInput>(EMPLOYEE_SEARCH);
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
                var result = await HRDataService.ListEmployeesAsync(input);
                ApplicationContext.SetSessionData(EMPLOYEE_SEARCH, input);
                return View(result);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Bổ sung nhân viên
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            try
            {
                ViewBag.Title = "Bổ sung nhân viên";
                var model = new Employee()
                {
                    EmployeeID = 0,
                    IsWorking = true
                };
                return View("Edit", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                ViewBag.Title = "Cập nhật thông tin nhân viên";
                var model = await HRDataService.GetEmployeeAsync(id);
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

        [HttpPost]
        public async Task<IActionResult> SaveData(Employee data, IFormFile? uploadPhoto)
        {
            try
            {
                ViewBag.Title = data.EmployeeID == 0 ? "Bổ sung nhân viên" : "Cập nhật thông tin nhân viên";

                //Kiểm tra dữ liệu đầu vào: FullName và Email là bắt buộc, Email chưa được sử dụng bởi nhân viên khác
                if (string.IsNullOrWhiteSpace(data.FullName))
                    ModelState.AddModelError(nameof(data.FullName), "Vui lòng nhập họ tên nhân viên");

                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError(nameof(data.Email), "Vui lòng nhập email nhân viên");
                else if (!await HRDataService.ValidateEmployeeEmailAsync(data.Email, data.EmployeeID))
                    ModelState.AddModelError(nameof(data.Email), "Email đã được sử dụng bởi nhân viên khác");

                if (!ModelState.IsValid)
                    return View("Edit", data);

                //Xử lý upload ảnh
                if (uploadPhoto != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(uploadPhoto.FileName)}";
                    var filePath = Path.Combine(ApplicationContext.WWWRootPath, "images/employees", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadPhoto.CopyToAsync(stream);
                    }
                    data.Photo = fileName;
                }

                //Tiền xử lý dữ liệu trước khi lưu vào database
                if (string.IsNullOrEmpty(data.Address)) data.Address = "";
                if (string.IsNullOrEmpty(data.Phone)) data.Phone = "";
                if (string.IsNullOrEmpty(data.Photo)) data.Photo = "nophoto.png";

                //Lưu dữ liệu vào database (bổ sung hoặc cập nhật)
                if (data.EmployeeID == 0)
                {
                    await HRDataService.AddEmployeeAsync(data);
                }
                else
                {
                    await HRDataService.UpdateEmployeeAsync(data);
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Ghi log lỗi căn cứ vào ex.Message và ex.StackTrace
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận hoặc dữ liệu không hợp lệ. Vui lòng kiểm tra dữ liệu hoặc thử lại sau");
                return View("Edit", data);
            }
        }

        /// <summary>
        /// Xóa nhân viên
        /// </summary>
        /// <param name="id">Mã nhân viên cần xóa</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                ViewBag.Title = "Xóa nhân viên";
                if (Request.Method == "POST")
                {
                    await HRDataService.DeleteEmployeeAsync(id);
                    return RedirectToAction("Index");
                }

                var model = await HRDataService.GetEmployeeAsync(id);
                if (model == null)
                    return RedirectToAction("Index");

                ViewBag.CanDelete = !await HRDataService.IsUsedEmployeeAsync(id);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Thay dổi mật khẩu cho account nhân viên
        /// </summary>
        /// <param name="id">Mã nhân viên có account cần thay dổi mật khẩu</param>
        /// <returns></returns>
        public async Task<IActionResult> ChangePassword(int id)
        {
            try
            {
                var employee = await HRDataService.GetEmployeeAsync(id);
                if (employee == null)
                    return RedirectToAction("Index");

                ViewBag.EmployeeID = id;
                ViewBag.EmployeeName = employee.FullName;

                ViewBag.IsWorking = employee.IsWorking;

                return View();
            }
            catch
            {
                ModelState.AddModelError("", "Hệ thống lỗi");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Xử lý đổi mật khẩu cho nhân viên
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ChangePassword(int id, string newPassword, string confirmPassword)
        {
            try
            {
                // Validate
                if (string.IsNullOrEmpty(newPassword))
                    ModelState.AddModelError("newPassword", "Vui lòng nhập mật khẩu");

                if (string.IsNullOrEmpty(confirmPassword))
                    ModelState.AddModelError("confirmPassword", "Vui lòng nhập lại mật khẩu");

                if (newPassword != confirmPassword)
                    ModelState.AddModelError("confirmPassword", "Mật khẩu xác nhận không đúng");

                // Lấy employee sớm để dùng lại
                var employee = await HRDataService.GetEmployeeAsync(id);
                if (employee == null)
                    return RedirectToAction("Index");

                if (!ModelState.IsValid)
                {
                    ViewBag.EmployeeID = id;
                    ViewBag.EmployeeName = employee.FullName;
                    ViewBag.EmployeeEmail = employee.Email;
                    ViewBag.IsWorking = employee.IsWorking;
                    return View();
                }

                // Hash
                string newPassMD5 = CryptHelper.HashMD5(newPassword);

                // Đổi mật khẩu
                bool result = await SecurityDataService.ChangePasswordAsync(employee.Email, newPassMD5);

                if (!result)
                {
                    ModelState.AddModelError("", "Không thể đổi mật khẩu");
                    ViewBag.EmployeeID = id;
                    ViewBag.EmployeeName = employee.FullName;
                    ViewBag.EmployeeEmail = employee.Email;
                    ViewBag.IsWorking = employee.IsWorking;
                    return View();
                }

                ModelState.Clear();

                ViewBag.Success = "Đổi mật khẩu nhân viên thành công";

                ViewBag.EmployeeID = id;
                ViewBag.EmployeeName = employee.FullName;
                ViewBag.EmployeeEmail = employee.Email;
                ViewBag.IsWorking = employee.IsWorking;

                return View();
            }
            catch
            {
                ModelState.AddModelError("", "Hệ thống đang xảy ra lỗi");
                return View();
            }
        }

        /// <summary>
        /// Thay đổi vai trò nhân viên
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult ChangeRole(int id)
        {
            try
            {
                ViewBag.Title = "Thay đổi vai trò của nhân viên";
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }
    }
}