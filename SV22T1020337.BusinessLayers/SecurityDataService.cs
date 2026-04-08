using SV22T1020337.DataLayers.Interfaces;
using SV22T1020337.DataLayers.SQLServer;
using SV22T1020337.Models.Security;

namespace SV22T1020337.BusinessLayers
{
    /// <summary>
    /// Lớp cung cấp các chức năng bảo mật (đăng nhập, đổi mật khẩu)
    /// </summary>
    public static class SecurityDataService
    {
        private static readonly IUserAccountRepository customerAccountDB;
        private static readonly IUserAccountRepository employeeAccountDB;

        /// <summary>
        /// Khởi tạo các repository
        /// </summary>
        static SecurityDataService()
        {
            string connectionString = Configuration.ConnectionString;

            customerAccountDB = new CustomerAccountRepository(connectionString);
            employeeAccountDB = new EmployeeAccountRepository(connectionString);
        }

        /// <summary>
        /// Kiểm tra thông tin đăng nhập
        /// </summary>
        public static async Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            try
            {
                // Kiểm tra tài khoản nhân viên
                var employee = await employeeAccountDB.AuthorizeAsync(userName, password);
                if (employee != null)
                    return employee;

                // Nếu không phải nhân viên thì kiểm tra khách hàng
                var customer = await customerAccountDB.AuthorizeAsync(userName, password);
                if (customer != null)
                    return customer;

                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static async Task<UserAccount?> AuthorizeCustomerAsync(string email, string password)
        {
            try
            {
                return await customerAccountDB.AuthorizeAsync(email, password);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Set mật khẩu cho khách hàng mới đăng ký (chỉ update bảng Customers)
        /// </summary>
        public static async Task<bool> SetCustomerPasswordAsync(string email, string password)
        {
            try
            {
                return await customerAccountDB.ChangePasswordAsync(email, password);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Đổi mật khẩu tài khoản
        /// </summary>
        public static async Task<bool> ChangePasswordAsync(string userName, string password)
        {
            try
            {
                // Thử đổi mật khẩu nhân viên
                bool result = await employeeAccountDB.ChangePasswordAsync(userName, password);
                if (result)
                    return true;

                // Nếu không phải nhân viên thì đổi cho khách hàng
                result = await customerAccountDB.ChangePasswordAsync(userName, password);

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}