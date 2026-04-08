using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020337.DataLayers.Interfaces;
using SV22T1020337.Models.Common;
using SV22T1020337.Models.Sales;

namespace SV22T1020337.DataLayers.SQLServer
{
    /// <summary>
    /// Thực hiện các thao tác truy xuất dữ liệu đối với đơn hàng
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<PagedResult<OrderViewInfo>> ListAsync(OrderSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            var result = new PagedResult<OrderViewInfo>()
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            string countSql = @"SELECT COUNT(*)
                     FROM Orders o
                     LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                     WHERE (@status = 0 OR o.Status = @status)
                     AND (@dateFrom IS NULL OR o.OrderTime >= @dateFrom)
                     AND (@dateTo IS NULL OR o.OrderTime <= @dateTo)
                     AND (@searchValue = '' OR c.CustomerName LIKE '%' + @searchValue + '%')
                     AND (@customerID = 0 OR o.CustomerID = @customerID)";

            result.RowCount = await connection.ExecuteScalarAsync<int>(countSql, new
            {
                status = (int)input.Status,
                dateFrom = input.DateFrom,
                dateTo = input.DateTo,
                searchValue = input.SearchValue ?? "",
                customerID = input.CustomerID
            });

            string sql = @"SELECT o.*, 
                       c.CustomerName, c.ContactName CustomerContactName,
                       c.Email CustomerEmail, c.Phone CustomerPhone, c.Address CustomerAddress,
                       e.FullName EmployeeName,
                       s.ShipperName, s.Phone ShipperPhone,
                       ISNULL((SELECT SUM(od.Quantity * od.SalePrice) FROM OrderDetails od WHERE od.OrderID = o.OrderID), 0) AS TotalAmount
                FROM Orders o
                LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
                LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
                WHERE (@status = 0 OR o.Status = @status)
                AND (@dateFrom IS NULL OR o.OrderTime >= @dateFrom)
                AND (@dateTo IS NULL OR o.OrderTime <= @dateTo)
                AND (@searchValue = '' OR c.CustomerName LIKE '%' + @searchValue + '%')
                AND (@customerID = 0 OR o.CustomerID = @customerID)
                ORDER BY o.OrderTime DESC
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            result.DataItems = (await connection.QueryAsync<OrderViewInfo>(sql, new
            {
                status = (int)input.Status,
                dateFrom = input.DateFrom,
                dateTo = input.DateTo,
                offset = input.Offset,
                pageSize = input.PageSize,
                searchValue = input.SearchValue ?? "",
                customerID = input.CustomerID
            })).ToList();

            return result;
        }

        public async Task<OrderViewInfo?> GetAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<OrderViewInfo>(
                @"SELECT o.*,
                         c.CustomerName,
                         c.ContactName AS CustomerContactName,
                         c.Email AS CustomerEmail,
                         c.Phone AS CustomerPhone,
                         c.Address AS CustomerAddress,
                         e.FullName AS EmployeeName,
                         s.ShipperName,
                         s.Phone AS ShipperPhone,
                         ISNULL((SELECT SUM(od.Quantity * od.SalePrice) FROM OrderDetails od WHERE od.OrderID = o.OrderID), 0) AS TotalAmount
                  FROM Orders o
                  LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                  LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
                  LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
                  WHERE o.OrderID=@orderID",
                new { orderID });
        }

        public async Task<int> AddAsync(Order data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO Orders
                          (CustomerID,OrderTime,DeliveryProvince,DeliveryAddress,
                           EmployeeID,AcceptTime,ShipperID,ShippedTime,FinishedTime,Status)
                          VALUES
                          (@CustomerID,@OrderTime,@DeliveryProvince,@DeliveryAddress,
                           @EmployeeID,@AcceptTime,@ShipperID,@ShippedTime,@FinishedTime,@Status);
                          SELECT SCOPE_IDENTITY();";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        public async Task<bool> UpdateAsync(Order data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"UPDATE Orders
                           SET CustomerID=@CustomerID,
                               DeliveryProvince=@DeliveryProvince,
                               DeliveryAddress=@DeliveryAddress,
                               EmployeeID=@EmployeeID,
                               AcceptTime=@AcceptTime,
                               ShipperID=@ShipperID,
                               ShippedTime=@ShippedTime,
                               FinishedTime=@FinishedTime,
                               Status=@Status
                           WHERE OrderID=@OrderID";

            return await connection.ExecuteAsync(sql, data) > 0;
        }

        public async Task<bool> DeleteAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(
                "DELETE FROM OrderDetails WHERE OrderID=@orderID",
                new { orderID });

            return await connection.ExecuteAsync(
                "DELETE FROM Orders WHERE OrderID=@orderID",
                new { orderID }) > 0;
        }

        public async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);

            var data = await connection.QueryAsync<OrderDetailViewInfo>(
                @"SELECT od.*,
                         p.ProductName,
                         p.Unit,
                         p.Photo
                  FROM OrderDetails od
                  JOIN Products p ON od.ProductID = p.ProductID
                  WHERE od.OrderID=@orderID",
                new { orderID });

            return data.ToList();
        }

        public async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<OrderDetailViewInfo>(
                @"SELECT od.*,
                         p.ProductName,
                         p.Unit,
                         p.Photo
                  FROM OrderDetails od
                  JOIN Products p ON od.ProductID = p.ProductID
                  WHERE od.OrderID=@orderID AND od.ProductID=@productID",
                new { orderID, productID });
        }

        public async Task<bool> AddDetailAsync(OrderDetail data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO OrderDetails
                          (OrderID,ProductID,Quantity,SalePrice)
                          VALUES
                          (@OrderID,@ProductID,@Quantity,@SalePrice)";

            return await connection.ExecuteAsync(sql, data) > 0;
        }

        public async Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"UPDATE OrderDetails
                           SET Quantity=@Quantity,
                               SalePrice=@SalePrice
                           WHERE OrderID=@OrderID
                           AND ProductID=@ProductID";

            return await connection.ExecuteAsync(sql, data) > 0;
        }

        public async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteAsync(
                @"DELETE FROM OrderDetails
                  WHERE OrderID=@orderID
                  AND ProductID=@productID",
                new { orderID, productID }) > 0;
        }
    }
}