using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020337.DataLayers.Interfaces;
using SV22T1020337.Models.Common;
using SV22T1020337.Models.Partner;

namespace SV22T1020337.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện các thao tác truy xuất dữ liệu đối với bảng Shippers
    /// trong cơ sở dữ liệu SQL Server.
    /// 
    /// Lớp này cài đặt interface IGenericRepository cho kiểu dữ liệu Shipper
    /// và sử dụng thư viện Dapper để thực hiện các truy vấn SQL.
    /// </summary>
    public class ShipperRepository : IGenericRepository<Shipper>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor của lớp ShipperRepository
        /// </summary>
        /// <param name="connectionString">
        /// Chuỗi kết nối đến cơ sở dữ liệu SQL Server
        /// </param>
        public ShipperRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy vấn danh sách người giao hàng theo điều kiện tìm kiếm
        /// và trả về kết quả phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả tìm kiếm dạng phân trang</returns>
        public async Task<PagedResult<Shipper>> ListEmployeesAsync(PaginationSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            string countSql = @"SELECT COUNT(*)
                                FROM Shippers
                                WHERE ShipperName LIKE @searchValue";

            string dataSql = @"SELECT *
                               FROM Shippers
                               WHERE ShipperName LIKE @searchValue
                               ORDER BY ShipperName
                               OFFSET @offset ROWS
                               FETCH NEXT @pageSize ROWS ONLY";

            var parameters = new
            {
                searchValue = $"%{input.SearchValue}%",
                offset = input.Offset,
                pageSize = input.PageSize
            };

            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            IEnumerable<Shipper> data;

            if (input.PageSize == 0)
            {
                data = await connection.QueryAsync<Shipper>(
                    @"SELECT *
                      FROM Shippers
                      WHERE ShipperName LIKE @searchValue
                      ORDER BY ShipperName",
                    parameters);
            }
            else
            {
                data = await connection.QueryAsync<Shipper>(dataSql, parameters);
            }

            return new PagedResult<Shipper>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin của một người giao hàng theo mã ShipperID
        /// </summary>
        /// <param name="id">Mã người giao hàng</param>
        /// <returns>
        /// Đối tượng Shipper nếu tìm thấy, ngược lại trả về null
        /// </returns>
        public async Task<Shipper?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT *
                           FROM Shippers
                           WHERE ShipperID = @id";

            return await connection.QueryFirstOrDefaultAsync<Shipper>(sql, new { id });
        }

        /// <summary>
        /// Bổ sung một người giao hàng mới vào cơ sở dữ liệu
        /// </summary>
        /// <param name="data">Thông tin người giao hàng cần thêm</param>
        /// <returns>Mã ShipperID của bản ghi vừa được thêm</returns>
        public async Task<int> AddAsync(Shipper data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO Shippers(ShipperName, Phone)
                           VALUES(@ShipperName, @Phone);
                           SELECT SCOPE_IDENTITY();";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Cập nhật thông tin của một người giao hàng
        /// </summary>
        /// <param name="data">Thông tin người giao hàng cần cập nhật</param>
        /// <returns>
        /// True nếu cập nhật thành công, False nếu không có bản ghi nào được cập nhật
        /// </returns>
        public async Task<bool> UpdateAsync(Shipper data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"UPDATE Shippers
                           SET ShipperName = @ShipperName,
                               Phone = @Phone
                           WHERE ShipperID = @ShipperID";

            int rows = await connection.ExecuteAsync(sql, data);

            return rows > 0;
        }

        /// <summary>
        /// Xóa một người giao hàng khỏi cơ sở dữ liệu
        /// </summary>
        /// <param name="id">Mã người giao hàng cần xóa</param>
        /// <returns>
        /// True nếu xóa thành công, False nếu không có dữ liệu bị xóa
        /// </returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"DELETE FROM Shippers
                           WHERE ShipperID = @id";

            int rows = await connection.ExecuteAsync(sql, new { id });

            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra xem người giao hàng có đang được sử dụng trong bảng Orders hay không
        /// </summary>
        /// <param name="id">Mã người giao hàng</param>
        /// <returns>
        /// True nếu người giao hàng đang được sử dụng,
        /// False nếu không có dữ liệu liên quan
        /// </returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT COUNT(*)
                           FROM Orders
                           WHERE ShipperID = @id";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });

            return count > 0;
        }
    }
}