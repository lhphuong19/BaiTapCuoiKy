using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020337.DataLayers.Interfaces;
using SV22T1020337.Models.Security;

namespace SV22T1020337.DataLayers.SQLServer
{
    /// <summary>
    /// Thực hiện xử lý đăng nhập và quản lý tài khoản của nhân viên
    /// </summary>
    public class EmployeeAccountRepository : IUserAccountRepository
    {
        private readonly string _connectionString;

        public EmployeeAccountRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Kiểm tra thông tin đăng nhập của nhân viên
        /// </summary>
        public async Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<UserAccount>(
                @"SELECT 
                        EmployeeID AS UserId,
                        Email AS UserName,
                        FullName AS DisplayName,
                        Email,
                        Photo,
                        'Employee' AS RoleNames
                  FROM Employees
                  WHERE Email = @userName
                        AND Password = @password
                        AND (IsWorking = 1 OR IsWorking IS NULL)",
                new { userName, password });
        }

        /// <summary>
        /// Đổi mật khẩu nhân viên
        /// </summary>
        public async Task<bool> ChangePasswordAsync(string userName, string password)
        {
            using var connection = new SqlConnection(_connectionString);

            int result = await connection.ExecuteAsync(
                @"UPDATE Employees
                  SET Password = @password
                  WHERE Email = @userName",
                new { userName, password });

            return result > 0;
        }
    }
}