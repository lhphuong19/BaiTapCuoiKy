using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020337.DataLayers.Interfaces;
using SV22T1020337.Models.Common;
using SV22T1020337.Models.Partner;

namespace SV22T1020337.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện các thao tác truy xuất dữ liệu đối với bảng Customers
    /// trong cơ sở dữ liệu SQL Server.
    /// 
    /// Lớp này cài đặt interface ICustomerRepository và sử dụng thư viện
    /// Dapper để thực hiện các truy vấn SQL.
    /// </summary>
    public class CustomerRepository : ICustomerRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor của lớp CustomerRepository
        /// </summary>
        /// <param name="connectionString">
        /// Chuỗi kết nối đến cơ sở dữ liệu SQL Server
        /// </param>
        public CustomerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy vấn danh sách khách hàng theo điều kiện tìm kiếm
        /// và trả về kết quả phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả tìm kiếm dạng phân trang</returns>
        public async Task<PagedResult<Customer>> ListEmployeesAsync(PaginationSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            string countSql = @"SELECT COUNT(*)
                                FROM Customers
                                WHERE CustomerName LIKE @searchValue
                                   OR ContactName LIKE @searchValue
                                   OR Phone LIKE @searchValue";

            string dataSql = @"SELECT CustomerID, CustomerName, ContactName,
                                      Province, Address, Phone, Email, IsLocked
                               FROM Customers
                               WHERE CustomerName LIKE @searchValue
                                  OR ContactName LIKE @searchValue
                                  OR Phone LIKE @searchValue
                               ORDER BY CustomerName
                               OFFSET @offset ROWS
                               FETCH NEXT @pageSize ROWS ONLY";

            var parameters = new
            {
                searchValue = $"%{input.SearchValue}%",
                offset = input.Offset,
                pageSize = input.PageSize
            };

            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            IEnumerable<Customer> data;

            if (input.PageSize == 0)
            {
                data = await connection.QueryAsync<Customer>(
                    @"SELECT CustomerID, CustomerName, ContactName,
                             Province, Address, Phone, Email, IsLocked
                      FROM Customers
                      WHERE CustomerName LIKE @searchValue
                         OR ContactName LIKE @searchValue
                         OR Phone LIKE @searchValue
                      ORDER BY CustomerName",
                    parameters);
            }
            else
            {
                data = await connection.QueryAsync<Customer>(dataSql, parameters);
            }

            return new PagedResult<Customer>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin của một khách hàng theo mã CustomerID
        /// </summary>
        /// <param name="id">Mã khách hàng</param>
        /// <returns>
        /// Đối tượng Customer nếu tìm thấy, ngược lại trả về null
        /// </returns>
        public async Task<Customer?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT CustomerID, CustomerName, ContactName,
                                  Province, Address, Phone, Email, IsLocked
                           FROM Customers
                           WHERE CustomerID = @id";

            return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { id });
        }

        /// <summary>
        /// Bổ sung một khách hàng mới vào cơ sở dữ liệu
        /// </summary>
        /// <param name="data">Thông tin khách hàng cần thêm</param>
        /// <returns>Mã CustomerID của bản ghi vừa được thêm</returns>
        public async Task<int> AddAsync(Customer data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO Customers
                           (CustomerName, ContactName, Province, Address, Phone, Email, IsLocked)
                           VALUES
                           (@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @IsLocked);
                           SELECT SCOPE_IDENTITY();";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Cập nhật thông tin của một khách hàng
        /// </summary>
        /// <param name="data">Thông tin khách hàng cần cập nhật</param>
        /// <returns>
        /// True nếu cập nhật thành công, False nếu không có bản ghi nào được cập nhật
        /// </returns>
        public async Task<bool> UpdateAsync(Customer data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"UPDATE Customers
                           SET CustomerName = @CustomerName,
                               ContactName = @ContactName,
                               Province = @Province,
                               Address = @Address,
                               Phone = @Phone,
                               Email = @Email,
                               IsLocked = @IsLocked
                           WHERE CustomerID = @CustomerID";

            int rows = await connection.ExecuteAsync(sql, data);

            return rows > 0;
        }

        /// <summary>
        /// Xóa một khách hàng khỏi cơ sở dữ liệu
        /// </summary>
        /// <param name="id">Mã khách hàng cần xóa</param>
        /// <returns>
        /// True nếu xóa thành công, False nếu không có dữ liệu bị xóa
        /// </returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"DELETE FROM Customers
                           WHERE CustomerID = @id";

            int rows = await connection.ExecuteAsync(sql, new { id });

            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra xem khách hàng có dữ liệu liên quan trong bảng Orders hay không
        /// </summary>
        /// <param name="id">Mã khách hàng</param>
        /// <returns>
        /// True nếu khách hàng đang được sử dụng,
        /// False nếu không có dữ liệu liên quan
        /// </returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT COUNT(*)
                           FROM Orders
                           WHERE CustomerID = @id";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });

            return count > 0;
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của email khách hàng
        /// </summary>
        /// <param name="email">Email cần kiểm tra</param>
        /// <param name="id">
        /// Nếu id = 0: kiểm tra email cho khách hàng mới
        /// Nếu id <> 0: kiểm tra email khi cập nhật khách hàng
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
                        FROM Customers
                        WHERE Email = @email";
            }
            else
            {
                sql = @"SELECT COUNT(*)
                        FROM Customers
                        WHERE Email = @email
                          AND CustomerID <> @id";
            }

            int count = await connection.ExecuteScalarAsync<int>(sql, new { email, id });

            return count == 0;
        }
    }
}