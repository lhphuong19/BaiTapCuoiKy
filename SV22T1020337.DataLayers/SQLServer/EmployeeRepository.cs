using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020337.DataLayers.Interfaces;
using SV22T1020337.Models.Common;
using SV22T1020337.Models.HR;

namespace SV22T1020337.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện các thao tác truy xuất dữ liệu đối với bảng Employees
    /// trong cơ sở dữ liệu SQL Server.
    /// 
    /// Lớp này cài đặt interface IEmployeeRepository và sử dụng thư viện
    /// Dapper để thực hiện các truy vấn SQL.
    /// </summary>
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor của lớp EmployeeRepository
        /// </summary>
        /// <param name="connectionString">
        /// Chuỗi kết nối đến cơ sở dữ liệu SQL Server
        /// </param>
        public EmployeeRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy vấn danh sách nhân viên theo điều kiện tìm kiếm
        /// và trả về kết quả phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả tìm kiếm dạng phân trang</returns>
        public async Task<PagedResult<Employee>> ListEmployeesAsync(PaginationSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            string countSql = @"SELECT COUNT(*)
                                FROM Employees
                                WHERE FullName LIKE @searchValue
                                   OR Phone LIKE @searchValue
                                   OR Email LIKE @searchValue";

            string dataSql = @"SELECT EmployeeID, FullName, BirthDate, Address,
                                      Phone, Email, Photo, IsWorking
                               FROM Employees
                               WHERE FullName LIKE @searchValue
                                  OR Phone LIKE @searchValue
                                  OR Email LIKE @searchValue
                               ORDER BY FullName
                               OFFSET @offset ROWS
                               FETCH NEXT @pageSize ROWS ONLY";

            var parameters = new
            {
                searchValue = $"%{input.SearchValue}%",
                offset = input.Offset,
                pageSize = input.PageSize
            };

            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            IEnumerable<Employee> data;

            if (input.PageSize == 0)
            {
                data = await connection.QueryAsync<Employee>(
                    @"SELECT EmployeeID, FullName, BirthDate, Address,
                             Phone, Email, Photo, IsWorking
                      FROM Employees
                      WHERE FullName LIKE @searchValue
                         OR Phone LIKE @searchValue
                         OR Email LIKE @searchValue
                      ORDER BY FullName",
                    parameters);
            }
            else
            {
                data = await connection.QueryAsync<Employee>(dataSql, parameters);
            }

            return new PagedResult<Employee>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin của một nhân viên theo mã EmployeeID
        /// </summary>
        /// <param name="id">Mã nhân viên</param>
        /// <returns>
        /// Đối tượng Employee nếu tìm thấy, ngược lại trả về null
        /// </returns>
        public async Task<Employee?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT EmployeeID, FullName, BirthDate, Address,
                                  Phone, Email, Photo, IsWorking
                           FROM Employees
                           WHERE EmployeeID = @id";

            return await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { id });
        }

        /// <summary>
        /// Bổ sung một nhân viên mới vào cơ sở dữ liệu
        /// </summary>
        /// <param name="data">Thông tin nhân viên cần thêm</param>
        /// <returns>Mã EmployeeID của bản ghi vừa được thêm</returns>
        public async Task<int> AddAsync(Employee data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO Employees
                           (FullName, BirthDate, Address, Phone, Email, Photo, IsWorking)
                           VALUES
                           (@FullName, @BirthDate, @Address, @Phone, @Email, @Photo, @IsWorking);
                           SELECT SCOPE_IDENTITY();";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Cập nhật thông tin của một nhân viên
        /// </summary>
        /// <param name="data">Thông tin nhân viên cần cập nhật</param>
        /// <returns>
        /// True nếu cập nhật thành công, False nếu không có bản ghi nào được cập nhật
        /// </returns>
        public async Task<bool> UpdateAsync(Employee data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"UPDATE Employees
                           SET FullName = @FullName,
                               BirthDate = @BirthDate,
                               Address = @Address,
                               Phone = @Phone,
                               Email = @Email,
                               Photo = @Photo,
                               IsWorking = @IsWorking
                           WHERE EmployeeID = @EmployeeID";

            int rows = await connection.ExecuteAsync(sql, data);

            return rows > 0;
        }

        /// <summary>
        /// Xóa một nhân viên khỏi cơ sở dữ liệu
        /// </summary>
        /// <param name="id">Mã nhân viên cần xóa</param>
        /// <returns>
        /// True nếu xóa thành công, False nếu không có dữ liệu bị xóa
        /// </returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"DELETE FROM Employees
                           WHERE EmployeeID = @id";

            int rows = await connection.ExecuteAsync(sql, new { id });

            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra xem nhân viên có dữ liệu liên quan trong bảng Orders hay không
        /// </summary>
        /// <param name="id">Mã nhân viên</param>
        /// <returns>
        /// True nếu nhân viên đang được sử dụng,
        /// False nếu không có dữ liệu liên quan
        /// </returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT COUNT(*)
                           FROM Orders
                           WHERE EmployeeID = @id";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });

            return count > 0;
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của email nhân viên
        /// </summary>
        /// <param name="email">Email cần kiểm tra</param>
        /// <param name="id">
        /// Nếu id = 0: kiểm tra email cho nhân viên mới
        /// Nếu id <> 0: kiểm tra email khi cập nhật nhân viên
        /// </param>
        /// <returns>
        /// True nếu email hợp lệ (không trùng),
        /// False nếu email đã tồn tại
        /// </returns>
        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql;

            if (id == 0)
            {
                sql = @"SELECT COUNT(*)
                        FROM Employees
                        WHERE Email = @email";
            }
            else
            {
                sql = @"SELECT COUNT(*)
                        FROM Employees
                        WHERE Email = @email
                          AND EmployeeID <> @id";
            }

            int count = await connection.ExecuteScalarAsync<int>(sql, new { email, id });

            return count == 0;
        }
    }
}