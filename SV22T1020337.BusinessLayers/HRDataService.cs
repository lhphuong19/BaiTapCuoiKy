using SV22T1020337.DataLayers.Interfaces;
using SV22T1020337.DataLayers.SQLServer;
using SV22T1020337.Models.Common;
using SV22T1020337.Models.HR;

namespace SV22T1020337.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến nhân sự của hệ thống    
    /// </summary>
    public static class HRDataService
    {
        private static readonly IEmployeeRepository employeeDB;

        /// <summary>
        /// Constructor
        /// </summary>
        static HRDataService()
        {
            employeeDB = new EmployeeRepository(Configuration.ConnectionString);
        }

        #region Employee

        /// <summary>
        /// Tìm kiếm và lấy danh sách nhân viên dưới dạng phân trang.
        /// </summary>
        /// <param name="input">
        /// Thông tin tìm kiếm và phân trang (từ khóa tìm kiếm, trang cần hiển thị, số dòng mỗi trang).
        /// </param>
        /// <returns>
        /// Kết quả tìm kiếm dưới dạng danh sách nhân viên có phân trang.
        /// </returns>
        public static async Task<PagedResult<Employee>> ListEmployeesAsync(PaginationSearchInput input)
        {
            try
            {
                return await employeeDB.ListEmployeesAsync(input);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một nhân viên dựa vào mã nhân viên.
        /// </summary>
        /// <param name="employeeID">Mã nhân viên cần tìm.</param>
        /// <returns>
        /// Đối tượng Employee nếu tìm thấy, ngược lại trả về null.
        /// </returns>
        public static async Task<Employee?> GetEmployeeAsync(int employeeID)
        {
            try
            {
                return await employeeDB.GetAsync(employeeID);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Bổ sung một nhân viên mới vào hệ thống.
        /// </summary>
        /// <param name="data">Thông tin nhân viên cần bổ sung.</param>
        /// <returns>Mã nhân viên được tạo mới.</returns>
        public static async Task<int> AddEmployeeAsync(Employee data)
        {
            try
            {
                if (data == null) return 0;
                if (string.IsNullOrWhiteSpace(data.FullName)) return 0;
                if (string.IsNullOrWhiteSpace(data.Email)) return 0;
                if (!await employeeDB.ValidateEmailAsync(data.Email, 0)) return 0;

                data.FullName = data.FullName.Trim();
                data.Email = data.Email.Trim();
                return await employeeDB.AddAsync(data);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Cập nhật thông tin của một nhân viên.
        /// </summary>
        /// <param name="data">Thông tin nhân viên cần cập nhật.</param>
        /// <returns>
        /// True nếu cập nhật thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> UpdateEmployeeAsync(Employee data)
        {
            try
            {
                if (data == null) return false;
                if (string.IsNullOrWhiteSpace(data.FullName)) return false;
                if (string.IsNullOrWhiteSpace(data.Email)) return false;
                if (!await employeeDB.ValidateEmailAsync(data.Email, data.EmployeeID)) return false;

                data.FullName = data.FullName.Trim();
                data.Email = data.Email.Trim();
                return await employeeDB.UpdateAsync(data);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Xóa một nhân viên dựa vào mã nhân viên.
        /// </summary>
        /// <param name="employeeID">Mã nhân viên cần xóa.</param>
        /// <returns>
        /// True nếu xóa thành công, False nếu nhân viên đang được sử dụng
        /// hoặc việc xóa không thực hiện được.
        /// </returns>
        public static async Task<bool> DeleteEmployeeAsync(int employeeID)
        {
            try
            {
                if (await employeeDB.IsUsedAsync(employeeID))
                    return false;

                return await employeeDB.DeleteAsync(employeeID);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Kiểm tra xem một nhân viên có đang được sử dụng trong dữ liệu hay không.
        /// </summary>
        /// <param name="employeeID">Mã nhân viên cần kiểm tra.</param>
        /// <returns>
        /// True nếu nhân viên đang được sử dụng, ngược lại False.
        /// </returns>
        public static async Task<bool> IsUsedEmployeeAsync(int employeeID)
        {
            try
            {
                return await employeeDB.IsUsedAsync(employeeID);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Kiểm tra xem email của nhân viên có hợp lệ không
        /// (không bị trùng với email của nhân viên khác).
        /// </summary>
        /// <param name="email">Địa chỉ email cần kiểm tra.</param>
        /// <param name="employeeID">
        /// Nếu employeeID = 0: kiểm tra email đối với nhân viên mới.
        /// Nếu employeeID khác 0: kiểm tra email của nhân viên có mã là employeeID.
        /// </param>
        /// <returns>
        /// True nếu email hợp lệ (không trùng), ngược lại False.
        /// </returns>
        public static async Task<bool> ValidateEmployeeEmailAsync(string email, int employeeID = 0)
        {
            try
            {
                return await employeeDB.ValidateEmailAsync(email, employeeID);
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}