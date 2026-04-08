using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020337.DataLayers.Interfaces;
using SV22T1020337.Models.Catalog;
using SV22T1020337.Models.Common;

namespace SV22T1020337.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện các thao tác truy xuất dữ liệu đối với mặt hàng
    /// (Products, ProductAttributes, ProductPhotos)
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProductRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Tìm kiếm danh sách mặt hàng có phân trang
        /// </summary>
        public async Task<PagedResult<Product>> ListAsync(ProductSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            if (input.MinPrice > 0 && input.MaxPrice > 0)
            {
                if (input.MinPrice > input.MaxPrice)
                {
                    var temp = input.MinPrice;
                    input.MinPrice = input.MaxPrice;
                    input.MaxPrice = temp;
                }
            }

            var parameters = new
            {
                search = $"%{input.SearchValue}%",
                input.CategoryID,
                input.SupplierID,
                input.MinPrice,
                input.MaxPrice,
                input.Offset,
                input.PageSize
            };

            string where = @" WHERE ProductName LIKE @search
                  AND (@CategoryID = 0 OR CategoryID = @CategoryID)
                  AND (@SupplierID = 0 OR SupplierID = @SupplierID)
                  AND (@MinPrice = 0 OR Price >= @MinPrice)
                  AND (@MaxPrice = 0 OR Price <= @MaxPrice) ";

            int rowCount = await connection.ExecuteScalarAsync<int>(
                $"SELECT COUNT(*) FROM Products {where}", parameters);

            IEnumerable<Product> data;

            if (input.PageSize == 0)
            {
                data = await connection.QueryAsync<Product>(
                    $"SELECT * FROM Products {where} ORDER BY ProductName",
                    parameters);
            }
            else
            {
                data = await connection.QueryAsync<Product>(
                    $@"SELECT * FROM Products {where}
                       ORDER BY ProductName
                       OFFSET @Offset ROWS
                       FETCH NEXT @PageSize ROWS ONLY",
                    parameters);
            }

            return new PagedResult<Product>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin 1 mặt hàng
        /// </summary>
        public async Task<Product?> GetAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<Product>(
                @"SELECT * FROM Products WHERE ProductID=@productID",
                new { productID });
        }

        /// <summary>
        /// Thêm mặt hàng
        /// </summary>
        public async Task<int> AddAsync(Product data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO Products
                          (ProductName,ProductDescription,SupplierID,CategoryID,
                           Unit,Price,Photo,IsSelling)
                          VALUES
                          (@ProductName,@ProductDescription,@SupplierID,@CategoryID,
                           @Unit,@Price,@Photo,@IsSelling);
                          SELECT SCOPE_IDENTITY();";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Cập nhật mặt hàng
        /// </summary>
        public async Task<bool> UpdateAsync(Product data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"UPDATE Products
                           SET ProductName=@ProductName,
                               ProductDescription=@ProductDescription,
                               SupplierID=@SupplierID,
                               CategoryID=@CategoryID,
                               Unit=@Unit,
                               Price=@Price,
                               Photo=@Photo,
                               IsSelling=@IsSelling
                           WHERE ProductID=@ProductID";

            return await connection.ExecuteAsync(sql, data) > 0;
        }

        /// <summary>
        /// Xóa mặt hàng
        /// </summary>
        public async Task<bool> DeleteAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteAsync(
                "DELETE FROM Products WHERE ProductID=@productID",
                new { productID }) > 0;
        }

        /// <summary>
        /// Kiểm tra mặt hàng có dữ liệu liên quan trong OrderDetails
        /// </summary>
        public async Task<bool> IsUsedAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            int count = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM OrderDetails WHERE ProductID=@productID",
                new { productID });

            return count > 0;
        }

        /// <summary>
        /// Danh sách thuộc tính mặt hàng
        /// </summary>
        public async Task<List<ProductAttribute>> ListAttributesAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            var data = await connection.QueryAsync<ProductAttribute>(
                @"SELECT * FROM ProductAttributes
                  WHERE ProductID=@productID
                  ORDER BY DisplayOrder",
                new { productID });

            return data.ToList();
        }

        /// <summary>
        /// Lấy 1 thuộc tính
        /// </summary>
        public async Task<ProductAttribute?> GetAttributeAsync(long attributeID)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<ProductAttribute>(
                "SELECT * FROM ProductAttributes WHERE AttributeID=@attributeID",
                new { attributeID });
        }

        /// <summary>
        /// Thêm thuộc tính
        /// </summary>
        public async Task<long> AddAttributeAsync(ProductAttribute data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO ProductAttributes
                          (ProductID,AttributeName,AttributeValue,DisplayOrder)
                          VALUES
                          (@ProductID,@AttributeName,@AttributeValue,@DisplayOrder);
                          SELECT SCOPE_IDENTITY();";

            return await connection.ExecuteScalarAsync<long>(sql, data);
        }

        /// <summary>
        /// Cập nhật thuộc tính
        /// </summary>
        public async Task<bool> UpdateAttributeAsync(ProductAttribute data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"UPDATE ProductAttributes
                           SET AttributeName=@AttributeName,
                               AttributeValue=@AttributeValue,
                               DisplayOrder=@DisplayOrder
                           WHERE AttributeID=@AttributeID";

            return await connection.ExecuteAsync(sql, data) > 0;
        }

        /// <summary>
        /// Xóa thuộc tính
        /// </summary>
        public async Task<bool> DeleteAttributeAsync(long attributeID)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteAsync(
                "DELETE FROM ProductAttributes WHERE AttributeID=@attributeID",
                new { attributeID }) > 0;
        }

        /// <summary>
        /// Danh sách ảnh mặt hàng
        /// </summary>
        public async Task<List<ProductPhoto>> ListPhotosAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            var data = await connection.QueryAsync<ProductPhoto>(
                @"SELECT * FROM ProductPhotos
                  WHERE ProductID=@productID
                  ORDER BY DisplayOrder",
                new { productID });

            return data.ToList();
        }

        /// <summary>
        /// Lấy thông tin 1 ảnh
        /// </summary>
        public async Task<ProductPhoto?> GetPhotoAsync(long photoID)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<ProductPhoto>(
                "SELECT * FROM ProductPhotos WHERE PhotoID=@photoID",
                new { photoID });
        }

        /// <summary>
        /// Thêm ảnh
        /// </summary>
        public async Task<long> AddPhotoAsync(ProductPhoto data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO ProductPhotos
                          (ProductID,Photo,Description,DisplayOrder,IsHidden)
                          VALUES
                          (@ProductID,@Photo,@Description,@DisplayOrder,@IsHidden);
                          SELECT SCOPE_IDENTITY();";

            return await connection.ExecuteScalarAsync<long>(sql, data);
        }

        /// <summary>
        /// Cập nhật ảnh
        /// </summary>
        public async Task<bool> UpdatePhotoAsync(ProductPhoto data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"UPDATE ProductPhotos
                           SET Photo=@Photo,
                               Description=@Description,
                               DisplayOrder=@DisplayOrder,
                               IsHidden=@IsHidden
                           WHERE PhotoID=@PhotoID";

            return await connection.ExecuteAsync(sql, data) > 0;
        }

        /// <summary>
        /// Xóa ảnh
        /// </summary>
        public async Task<bool> DeletePhotoAsync(long photoID)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteAsync(
                "DELETE FROM ProductPhotos WHERE PhotoID=@photoID",
                new { photoID }) > 0;
        }
    }
}