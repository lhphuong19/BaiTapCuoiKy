using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020337.DataLayers.Interfaces;
using SV22T1020337.Models.Common;
using SV22T1020337.Models.Partner;
using System.Data;

namespace SV22T1020337.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện các thao tác truy xuất dữ liệu đối với bảng Suppliers
    /// trong SQL Server.
    /// 
    /// Lớp này cài đặt interface IGenericRepository cho kiểu dữ liệu Supplier
    /// và sử dụng thư viện Dapper để thực hiện các truy vấn.
    /// </summary>
    public class SupplierRepository : IGenericRepository<Supplier>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor của lớp SupplierRepository
        /// </summary>
        /// <param name="connectionString">
        /// Chuỗi kết nối đến cơ sở dữ liệu SQL Server
        /// </param>
        public SupplierRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy vấn danh sách nhà cung cấp theo điều kiện tìm kiếm
        /// và trả về kết quả phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả tìm kiếm dạng phân trang</returns>
        public async Task<PagedResult<Supplier>> ListEmployeesAsync(PaginationSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            string countSql = @"SELECT COUNT(*)
                                FROM Suppliers
                                WHERE SupplierName LIKE @searchValue 
                                   OR ContactName LIKE @searchValue";

            string dataSql = @"SELECT *
                               FROM Suppliers
                               WHERE SupplierName LIKE @searchValue 
                                  OR ContactName LIKE @searchValue
                               ORDER BY SupplierName
                               OFFSET @offset ROWS
                               FETCH NEXT @pageSize ROWS ONLY";

            var parameters = new
            {
                searchValue = $"%{input.SearchValue}%",
                offset = input.Offset,
                pageSize = input.PageSize
            };

            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            IEnumerable<Supplier> data;

            if (input.PageSize == 0)
            {
                data = await connection.QueryAsync<Supplier>(
                    @"SELECT * 
                      FROM Suppliers
                      WHERE SupplierName LIKE @searchValue 
                         OR ContactName LIKE @searchValue
                      ORDER BY SupplierName",
                    parameters);
            }
            else
            {
                data = await connection.QueryAsync<Supplier>(dataSql, parameters);
            }

            return new PagedResult<Supplier>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin của một nhà cung cấp dựa vào mã SupplierID
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần lấy</param>
        /// <returns>
        /// Đối tượng Supplier nếu tìm thấy, ngược lại trả về null
        /// </returns>
        public async Task<Supplier?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT *
                           FROM Suppliers
                           WHERE SupplierID = @id";

            return await connection.QueryFirstOrDefaultAsync<Supplier>(sql, new { id });
        }

        /// <summary>
        /// Bổ sung một nhà cung cấp mới vào cơ sở dữ liệu
        /// </summary>
        /// <param name="data">Thông tin nhà cung cấp cần thêm</param>
        /// <returns>Mã SupplierID của bản ghi vừa được thêm</returns>
        public async Task<int> AddAsync(Supplier data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO Suppliers
                           (SupplierName, ContactName, Province, Address, Phone, Email)
                           VALUES
                           (@SupplierName, @ContactName, @Province, @Address, @Phone, @Email);
                           SELECT SCOPE_IDENTITY();";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Cập nhật thông tin của một nhà cung cấp trong cơ sở dữ liệu
        /// </summary>
        /// <param name="data">Thông tin nhà cung cấp cần cập nhật</param>
        /// <returns>
        /// True nếu cập nhật thành công, False nếu không có bản ghi nào được cập nhật
        /// </returns>
        public async Task<bool> UpdateAsync(Supplier data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"UPDATE Suppliers
                           SET SupplierName = @SupplierName,
                               ContactName = @ContactName,
                               Province = @Province,
                               Address = @Address,
                               Phone = @Phone,
                               Email = @Email
                           WHERE SupplierID = @SupplierID";

            int rows = await connection.ExecuteAsync(sql, data);

            return rows > 0;
        }

        /// <summary>
        /// Xóa một nhà cung cấp khỏi cơ sở dữ liệu theo SupplierID
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần xóa</param>
        /// <returns>
        /// True nếu xóa thành công, False nếu không có dữ liệu bị xóa
        /// </returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"DELETE FROM Suppliers
                           WHERE SupplierID = @id";

            int rows = await connection.ExecuteAsync(sql, new { id });

            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra xem nhà cung cấp có đang được sử dụng trong bảng Products hay không
        /// </summary>
        /// <param name="id">Mã nhà cung cấp</param>
        /// <returns>
        /// True nếu nhà cung cấp đang được sử dụng,
        /// False nếu không có dữ liệu liên quan
        /// </returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT COUNT(*)
                           FROM Products
                           WHERE SupplierID = @id";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });

            return count > 0;
        }
    }
}