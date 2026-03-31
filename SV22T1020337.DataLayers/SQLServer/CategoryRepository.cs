using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020337.DataLayers.Interfaces;
using SV22T1020337.Models.Common;
using SV22T1020337.Models.Catalog;

namespace SV22T1020337.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện các thao tác truy xuất dữ liệu đối với bảng Categories
    /// trong cơ sở dữ liệu SQL Server.
    /// 
    /// Lớp này cài đặt interface IGenericRepository cho kiểu dữ liệu Category
    /// và sử dụng thư viện Dapper để thực hiện các truy vấn SQL.
    /// </summary>
    public class CategoryRepository : IGenericRepository<Category>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor của lớp CategoryRepository
        /// </summary>
        /// <param name="connectionString">
        /// Chuỗi kết nối đến cơ sở dữ liệu SQL Server
        /// </param>
        public CategoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy vấn danh sách loại hàng theo điều kiện tìm kiếm
        /// và trả về kết quả phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả tìm kiếm dạng phân trang</returns>
        public async Task<PagedResult<Category>> ListEmployeesAsync(PaginationSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            string countSql = @"SELECT COUNT(*)
                                FROM Categories
                                WHERE CategoryName LIKE @searchValue";

            string dataSql = @"SELECT *
                               FROM Categories
                               WHERE CategoryName LIKE @searchValue
                               ORDER BY CategoryName
                               OFFSET @offset ROWS
                               FETCH NEXT @pageSize ROWS ONLY";

            var parameters = new
            {
                searchValue = $"%{input.SearchValue}%",
                offset = input.Offset,
                pageSize = input.PageSize
            };

            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            IEnumerable<Category> data;

            if (input.PageSize == 0)
            {
                data = await connection.QueryAsync<Category>(
                    @"SELECT *
                      FROM Categories
                      WHERE CategoryName LIKE @searchValue
                      ORDER BY CategoryName",
                    parameters);
            }
            else
            {
                data = await connection.QueryAsync<Category>(dataSql, parameters);
            }

            return new PagedResult<Category>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin của một loại hàng theo mã CategoryID
        /// </summary>
        /// <param name="id">Mã loại hàng</param>
        /// <returns>
        /// Đối tượng Category nếu tìm thấy, ngược lại trả về null
        /// </returns>
        public async Task<Category?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT *
                           FROM Categories
                           WHERE CategoryID = @id";

            return await connection.QueryFirstOrDefaultAsync<Category>(sql, new { id });
        }

        /// <summary>
        /// Bổ sung một loại hàng mới vào cơ sở dữ liệu
        /// </summary>
        /// <param name="data">Thông tin loại hàng cần thêm</param>
        /// <returns>Mã CategoryID của bản ghi vừa được thêm</returns>
        public async Task<int> AddAsync(Category data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO Categories(CategoryName, Description)
                           VALUES(@CategoryName, @Description);
                           SELECT SCOPE_IDENTITY();";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Cập nhật thông tin của một loại hàng
        /// </summary>
        /// <param name="data">Thông tin loại hàng cần cập nhật</param>
        /// <returns>
        /// True nếu cập nhật thành công, False nếu không có bản ghi nào được cập nhật
        /// </returns>
        public async Task<bool> UpdateAsync(Category data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"UPDATE Categories
                           SET CategoryName = @CategoryName,
                               Description = @Description
                           WHERE CategoryID = @CategoryID";

            int rows = await connection.ExecuteAsync(sql, data);

            return rows > 0;
        }

        /// <summary>
        /// Xóa một loại hàng khỏi cơ sở dữ liệu
        /// </summary>
        /// <param name="id">Mã loại hàng cần xóa</param>
        /// <returns>
        /// True nếu xóa thành công, False nếu không có dữ liệu bị xóa
        /// </returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"DELETE FROM Categories
                           WHERE CategoryID = @id";

            int rows = await connection.ExecuteAsync(sql, new { id });

            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra xem loại hàng có đang được sử dụng trong bảng Products hay không
        /// </summary>
        /// <param name="id">Mã loại hàng</param>
        /// <returns>
        /// True nếu loại hàng đang được sử dụng,
        /// False nếu không có dữ liệu liên quan
        /// </returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT COUNT(*)
                           FROM Products
                           WHERE CategoryID = @id";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });

            return count > 0;
        }
    }
}