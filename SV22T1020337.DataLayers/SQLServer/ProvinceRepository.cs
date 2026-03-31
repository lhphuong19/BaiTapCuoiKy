using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020337.DataLayers.Interfaces;
using SV22T1020337.Models.DataDictionary;

namespace SV22T1020337.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện truy xuất dữ liệu từ bảng Provinces
    /// trong cơ sở dữ liệu SQL Server.
    /// 
    /// Lớp này cài đặt interface IDataDictionaryRepository
    /// để cung cấp danh sách các tỉnh/thành phố trong hệ thống.
    /// </summary>
    public class ProvinceRepository : IDataDictionaryRepository<Province>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor của lớp ProvinceRepository
        /// </summary>
        /// <param name="connectionString">
        /// Chuỗi kết nối đến cơ sở dữ liệu SQL Server
        /// </param>
        public ProvinceRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Lấy danh sách tất cả các tỉnh/thành phố trong hệ thống
        /// </summary>
        /// <returns>Danh sách các tỉnh/thành phố</returns>
        public async Task<List<Province>> ListAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT ProvinceName
                           FROM Provinces
                           ORDER BY ProvinceName";

            var data = await connection.QueryAsync<Province>(sql);

            return data.ToList();
        }
    }
}