using SV22T1020337.DataLayers.Interfaces;
using SV22T1020337.DataLayers.SQLServer;
using SV22T1020337.Models.Catalog;
using SV22T1020337.Models.Common;
using SV22T1020337.Models.DataDictionary;
using SV22T1020337.Models.Partner;
using SV22T1020337.Models.Sales;

namespace SV22T1020337.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến bán hàng
    /// bao gồm: đơn hàng (Order) và chi tiết đơn hàng (OrderDetail).
    /// </summary>
    public static class SalesDataService
    {
        private static readonly IOrderRepository orderDB;

        /// <summary>
        /// Constructor
        /// </summary>
        static SalesDataService()
        {
            orderDB = new OrderRepository(Configuration.ConnectionString);
        }

        #region Order

        /// <summary>
        /// Tìm kiếm và lấy danh sách đơn hàng dưới dạng phân trang
        /// </summary>
        public static async Task<PagedResult<OrderViewInfo>> ListOrdersAsync(OrderSearchInput input)
        {
            {
                try
                {
                    return await orderDB.ListAsync(input);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một đơn hàng
        /// </summary>
        public static async Task<OrderViewInfo?> GetOrderAsync(int orderID)
        {
            try
            {
                return await orderDB.GetAsync(orderID);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Tạo đơn hàng mới
        /// </summary>
        //public static async Task<int> AddOrderAsync(Order data)
        //{
        //    data.Status = OrderStatusEnum.New;
        //    data.OrderTime = DateTime.Now;

        //    return await orderDB.AddAsync(data);
        //}

        public static async Task<int> AddOrderAsync(int customerID = 0, string province = "", string address = "")
        {
            try
            {
                var order = new Order()
                {
                    CustomerID = customerID,
                    DeliveryAddress = address,
                    DeliveryProvince = province,
                    Status = OrderStatusEnum.New,
                    OrderTime = DateTime.Now
                };
                return await orderDB.AddAsync(order);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Cập nhật thông tin đơn hàng
        /// </summary>
        public static async Task<bool> UpdateOrderAsync(Order data)
        {
            try
            {
                if (data == null) return false;

                var existingOrder = await orderDB.GetAsync(data.OrderID);
                if (existingOrder == null) return false;

                if (existingOrder.Status != OrderStatusEnum.New &&
                    existingOrder.Status != OrderStatusEnum.Accepted)
                    return false;

                return await orderDB.UpdateAsync(data);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Xóa đơn hàng
        /// </summary>
        public static async Task<bool> DeleteOrderAsync(int orderID)
        {
            try
            {
                var order = await orderDB.GetAsync(orderID);
                if (order == null) return false;

                if (order.Status != OrderStatusEnum.New)
                    return false;

                return await orderDB.DeleteAsync(orderID);
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Order Status Processing

        /// <summary>
        /// Duyệt đơn hàng
        /// </summary>
        public static async Task<bool> AcceptOrderAsync(int orderID, int employeeID)
        {
            try
            {
                var order = await orderDB.GetAsync(orderID);
                if (order == null) return false;
                if (order.Status != OrderStatusEnum.New) return false;

                order.EmployeeID = employeeID;
                order.AcceptTime = DateTime.Now;
                order.Status = OrderStatusEnum.Accepted;

                return await orderDB.UpdateAsync(order);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Từ chối đơn hàng
        /// </summary>
        public static async Task<bool> RejectOrderAsync(int orderID, int employeeID)
        {
            try
            {
                var order = await orderDB.GetAsync(orderID);
                if (order == null) return false;
                if (order.Status != OrderStatusEnum.New) return false;

                order.EmployeeID = employeeID;
                order.FinishedTime = DateTime.Now;
                order.Status = OrderStatusEnum.Rejected;

                return await orderDB.UpdateAsync(order);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Hủy đơn hàng
        /// </summary>
        public static async Task<bool> CancelOrderAsync(int orderID)
        {
            try
            {
                var order = await orderDB.GetAsync(orderID);
                if (order == null) return false;

                if (order.Status != OrderStatusEnum.New &&
                    order.Status != OrderStatusEnum.Accepted)
                    return false;

                order.FinishedTime = DateTime.Now;
                order.Status = OrderStatusEnum.Cancelled;

                return await orderDB.UpdateAsync(order);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Giao đơn hàng cho người giao hàng
        /// </summary>
        public static async Task<bool> ShipOrderAsync(int orderID, int shipperID)
        {
            try
            {
                var order = await orderDB.GetAsync(orderID);
                if (order == null) return false;
                if (order.Status != OrderStatusEnum.Accepted) return false;

                order.ShipperID = shipperID;
                order.ShippedTime = DateTime.Now;
                order.Status = OrderStatusEnum.Shipping;

                return await orderDB.UpdateAsync(order);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Hoàn tất đơn hàng
        /// </summary>
        public static async Task<bool> CompleteOrderAsync(int orderID)
        {
            try
            {
                var order = await orderDB.GetAsync(orderID);
                if (order == null) return false;
                if (order.Status != OrderStatusEnum.Shipping) return false;

                order.FinishedTime = DateTime.Now;
                order.Status = OrderStatusEnum.Completed;

                return await orderDB.UpdateAsync(order);
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Order Detail

        /// <summary>
        /// Lấy danh sách mặt hàng của đơn hàng
        /// </summary>
        public static async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            try
            {
                return await orderDB.ListDetailsAsync(orderID);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Lấy thông tin một mặt hàng trong đơn hàng
        /// </summary>
        public static async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            try
            {
                return await orderDB.GetDetailAsync(orderID, productID);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Thêm mặt hàng vào đơn hàng
        /// </summary>
        public static async Task<bool> AddDetailAsync(OrderDetail data)
        {
            try
            {
                // Kiểm tra dữ liệu và trạng thái đơn hàng trước khi thêm mặt hàng
                if (data == null) return false;
                if (data.Quantity <= 0) return false;
                if (data.SalePrice <= 0) return false;

                //var order = await orderDB.GetAsync(data.OrderID);
                //if (order == null) return false;

                //// Chỉ được thêm mặt hàng khi đơn hàng ở trạng thái New
                //if (order.Status != OrderStatusEnum.New)
                //    return false;

                return await orderDB.AddDetailAsync(data);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Cập nhật mặt hàng trong đơn hàng
        /// </summary>
        public static async Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            try
            {
                // Kiểm tra dữ liệu và trạng thái đơn hàng trước khi cập nhật mặt hàng
                if (data == null) return false;
                if (data.Quantity <= 0) return false;
                if (data.SalePrice <= 0) return false;

                var order = await orderDB.GetAsync(data.OrderID);
                if (order == null) return false;

                // Chỉ được cập nhật mặt hàng khi đơn hàng ở trạng thái New
                if (order.Status != OrderStatusEnum.New)
                    return false;

                return await orderDB.UpdateDetailAsync(data);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Xóa mặt hàng khỏi đơn hàng
        /// </summary>
        public static async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            try
            {
                // Kiểm tra trạng thái đơn hàng trước khi xóa mặt hàng
                var order = await orderDB.GetAsync(orderID);
                if (order == null) return false;

                // Chỉ được xóa mặt hàng khi đơn hàng ở trạng thái New
                if (order.Status != OrderStatusEnum.New)
                    return false;

                return await orderDB.DeleteDetailAsync(orderID, productID);
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}