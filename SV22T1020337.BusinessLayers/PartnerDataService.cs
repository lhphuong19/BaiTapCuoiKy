using SV22T1020337.BusinessLayers;
using SV22T1020337.DataLayers.Interfaces;
using SV22T1020337.DataLayers.SQLServer;
using SV22T1020337.Models.Common;
using SV22T1020337.Models.Partner;

/// <summary>
/// Cung cấp các chức năng xử lý dữ liệu liên quan đến các đối tác của hệ thống
/// bao gồm: nhà cung cấp (Supplier), khách hàng (Customer) và người giao hàng (Shipper)
/// </summary>
public static class PartnerDataService
{
    private static readonly IGenericRepository<Supplier> supplierDB;
    private static readonly ICustomerRepository customerDB;
    private static readonly IGenericRepository<Shipper> shipperDB;

    /// <summary>
    /// Ctor
    /// </summary>
    static PartnerDataService()
    {
        supplierDB = new SupplierRepository(Configuration.ConnectionString);
        customerDB = new CustomerRepository(Configuration.ConnectionString);
        shipperDB = new ShipperRepository(Configuration.ConnectionString);
    }

    #region Supplier

    /// <summary>
    /// Tìm kiếm và lấy danh sách nhà cung cấp dưới dạng phân trang.
    /// </summary>
    /// <param name="input">
    /// Thông tin tìm kiếm và phân trang (từ khóa tìm kiếm, trang cần hiển thị, số dòng mỗi trang).
    /// </param>
    /// <returns>
    /// Kết quả tìm kiếm dưới dạng danh sách nhà cung cấp có phân trang.
    /// </returns>
    public static async Task<PagedResult<Supplier>> ListSuppliersAsync(PaginationSearchInput input)
    {
        try
        {
            return await supplierDB.ListEmployeesAsync(input);
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một nhà cung cấp dựa vào mã nhà cung cấp.
    /// </summary>
    /// <param name="supplierID">Mã nhà cung cấp cần tìm.</param>
    /// <returns>
    /// Đối tượng Supplier nếu tìm thấy, ngược lại trả về null.
    /// </returns>
    public static async Task<Supplier?> GetSupplierAsync(int supplierID)
    {
        try
        {
            return await supplierDB.GetAsync(supplierID);
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Bổ sung một nhà cung cấp mới vào hệ thống.
    /// </summary>
    /// <param name="data">Thông tin nhà cung cấp cần bổ sung.</param>
    /// <returns>Mã nhà cung cấp được tạo mới.</returns>
    public static async Task<int> AddSupplierAsync(Supplier data)
    {
        try
        {
            if (data == null) return 0;
            if (string.IsNullOrWhiteSpace(data.SupplierName)) return 0;
            if (string.IsNullOrWhiteSpace(data.ContactName)) return 0;

            data.SupplierName = data.SupplierName.Trim();
            data.ContactName = data.ContactName.Trim();
            return await supplierDB.AddAsync(data);
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Cập nhật thông tin của một nhà cung cấp.
    /// </summary>
    /// <param name="data">Thông tin nhà cung cấp cần cập nhật.</param>
    /// <returns>
    /// True nếu cập nhật thành công, ngược lại False.
    /// </returns>
    public static async Task<bool> UpdateSupplierAsync(Supplier data)
    {
        try
        {
            if (data == null) return false;
            if (string.IsNullOrWhiteSpace(data.SupplierName)) return false;
            if (string.IsNullOrWhiteSpace(data.ContactName)) return false;

            data.SupplierName = data.SupplierName.Trim();
            data.ContactName = data.ContactName.Trim();
            return await supplierDB.UpdateAsync(data);
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Xóa một nhà cung cấp dựa vào mã nhà cung cấp.
    /// </summary>
    /// <param name="supplierID">Mã nhà cung cấp cần xóa.</param>
    /// <returns>
    /// True nếu xóa thành công, False nếu nhà cung cấp đang được sử dụng
    /// hoặc việc xóa không thực hiện được.
    /// </returns>
    public static async Task<bool> DeleteSupplierAsync(int supplierID)
    {
        try
        {
            if (await supplierDB.IsUsedAsync(supplierID))
                return false;

            return await supplierDB.DeleteAsync(supplierID);
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Kiểm tra xem một nhà cung cấp có đang được sử dụng trong dữ liệu hay không.
    /// </summary>
    /// <param name="supplierID">Mã nhà cung cấp cần kiểm tra.</param>
    /// <returns>
    /// True nếu nhà cung cấp đang được sử dụng, ngược lại False.
    /// </returns>
    public static async Task<bool> IsUsedSupplierAsync(int supplierID)
    {
        try
        {
            return await supplierDB.IsUsedAsync(supplierID);
        }
        catch
        {
            throw;
        }
    }

    #endregion

    #region Customer

    /// <summary>
    /// Tìm kiếm và lấy danh sách khách hàng dưới dạng phân trang.
    /// </summary>
    /// <param name="input">
    /// Thông tin tìm kiếm và phân trang.
    /// </param>
    /// <returns>
    /// Danh sách khách hàng phù hợp với điều kiện tìm kiếm.
    /// </returns>
    public static async Task<PagedResult<Customer>> ListCustomersAsync(PaginationSearchInput input)
    {
        try
        {
            return await customerDB.ListEmployeesAsync(input);
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một khách hàng dựa vào mã khách hàng.
    /// </summary>
    /// <param name="customerID">Mã khách hàng cần tìm.</param>
    /// <returns>
    /// Đối tượng Customer nếu tìm thấy, ngược lại trả về null.
    /// </returns>
    public static async Task<Customer?> GetCustomerAsync(int customerID)
    {
        try
        {
            return await customerDB.GetAsync(customerID);
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Bổ sung một khách hàng mới vào hệ thống.
    /// </summary>
    /// <param name="data">Thông tin khách hàng cần bổ sung.</param>
    /// <returns>Mã khách hàng được tạo mới.</returns>
    public static async Task<int> AddCustomerAsync(Customer data)
    {
        try
        {
            if (data == null) return 0;
            if (string.IsNullOrWhiteSpace(data.CustomerName)) return 0;
            if (string.IsNullOrWhiteSpace(data.ContactName)) return 0;
            if (string.IsNullOrWhiteSpace(data.Email)) return 0;
            if (!await customerDB.ValidateEmailAsync(data.Email, 0)) return 0;

            data.CustomerName = data.CustomerName.Trim();
            data.ContactName = data.ContactName.Trim();
            data.Email = data.Email.Trim();
            return await customerDB.AddAsync(data);
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Cập nhật thông tin của một khách hàng.
    /// </summary>
    /// <param name="data">Thông tin khách hàng cần cập nhật.</param>
    /// <returns>
    /// True nếu cập nhật thành công, ngược lại False.
    /// </returns>
    public static async Task<bool> UpdateCustomerAsync(Customer data)
    {
        try
        {
            if (data == null) return false;
            if (string.IsNullOrWhiteSpace(data.CustomerName)) return false;
            if (string.IsNullOrWhiteSpace(data.ContactName)) return false;
            if (string.IsNullOrWhiteSpace(data.Email)) return false;
            if (!await customerDB.ValidateEmailAsync(data.Email, data.CustomerID)) return false;

            data.CustomerName = data.CustomerName.Trim();
            data.ContactName = data.ContactName.Trim();
            data.Email = data.Email.Trim();
            return await customerDB.UpdateAsync(data);
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Xóa một khách hàng dựa vào mã khách hàng.
    /// </summary>
    /// <param name="customerID">Mã khách hàng cần xóa.</param>
    /// <returns>
    /// True nếu xóa thành công, False nếu khách hàng đang được sử dụng
    /// hoặc việc xóa không thực hiện được.
    /// </returns>
    public static async Task<bool> DeleteCustomerAsync(int customerID)
    {
        try
        {
            if (await customerDB.IsUsedAsync(customerID))
                return false;

            return await customerDB.DeleteAsync(customerID);
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Kiểm tra xem một khách hàng có đang được sử dụng trong dữ liệu hay không.
    /// </summary>
    /// <param name="customerID">Mã khách hàng cần kiểm tra.</param>
    /// <returns>
    /// True nếu khách hàng đang được sử dụng, ngược lại False.
    /// </returns>
    public static async Task<bool> IsUsedCustomerAsync(int customerID)
    {
        try
        {
            return await customerDB.IsUsedAsync(customerID);
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Kiểm tra xem email của khách hàng có hợp lệ không
    /// </summary>
    /// <param name="email">Địa chỉ email cần kiểm tra</param>
    /// <param name="customerID">
    /// Bằng 0 nếu kiểm tra email đối với khách hàng mới.
    /// Khác 0 nếu kiểm tra email của khách hàng có mã là <paramref name="customerID"/>
    /// </param>
    /// <returns></returns>
    public static async Task<bool> ValidatelCustomerEmailAsync(string email, int customerID = 0)
    {
        try
        {
            return await customerDB.ValidateEmailAsync(email, customerID);
        }
        catch
        {
            throw;
        }
    }

    #endregion

    #region Shipper

    /// <summary>
    /// Tìm kiếm và lấy danh sách người giao hàng dưới dạng phân trang.
    /// </summary>
    /// <param name="input">
    /// Thông tin tìm kiếm và phân trang.
    /// </param>
    /// <returns>
    /// Danh sách người giao hàng phù hợp với điều kiện tìm kiếm.
    /// </returns>
    public static async Task<PagedResult<Shipper>> ListShippersAsync(PaginationSearchInput input)
    {
        try
        {
            return await shipperDB.ListEmployeesAsync(input);
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một người giao hàng dựa vào mã người giao hàng.
    /// </summary>
    /// <param name="shipperID">Mã người giao hàng cần tìm.</param>
    /// <returns>
    /// Đối tượng Shipper nếu tìm thấy, ngược lại trả về null.
    /// </returns>
    public static async Task<Shipper?> GetShipperAsync(int shipperID)
    {
        try
        {
            return await shipperDB.GetAsync(shipperID);
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Bổ sung một người giao hàng mới vào hệ thống.
    /// </summary>
    /// <param name="data">Thông tin người giao hàng cần bổ sung.</param>
    /// <returns>Mã người giao hàng được tạo mới.</returns>
    public static async Task<int> AddShipperAsync(Shipper data)
    {
        try
        {
            if (data == null) return 0;
            if (string.IsNullOrWhiteSpace(data.ShipperName)) return 0;

            data.ShipperName = data.ShipperName.Trim();
            return await shipperDB.AddAsync(data);
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Cập nhật thông tin của một người giao hàng.
    /// </summary>
    /// <param name="data">Thông tin người giao hàng cần cập nhật.</param>
    /// <returns>
    /// True nếu cập nhật thành công, ngược lại False.
    /// </returns>
    public static async Task<bool> UpdateShipperAsync(Shipper data)
    {
        try
        {
            if (data == null) return false;
            if (string.IsNullOrWhiteSpace(data.ShipperName)) return false;

            data.ShipperName = data.ShipperName.Trim();
            return await shipperDB.UpdateAsync(data);
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Xóa một người giao hàng dựa vào mã người giao hàng.
    /// </summary>
    /// <param name="shipperID">Mã người giao hàng cần xóa.</param>
    /// <returns>
    /// True nếu xóa thành công, False nếu người giao hàng đang được sử dụng
    /// hoặc việc xóa không thực hiện được.
    /// </returns>
    public static async Task<bool> DeleteShipperAsync(int shipperID)
    {
        try
        {
            if (await shipperDB.IsUsedAsync(shipperID))
                return false;

            return await shipperDB.DeleteAsync(shipperID);
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Kiểm tra xem một người giao hàng có đang được sử dụng trong dữ liệu hay không.
    /// </summary>
    /// <param name="shipperID">Mã người giao hàng cần kiểm tra.</param>
    /// <returns>
    /// True nếu người giao hàng đang được sử dụng, ngược lại False.
    /// </returns>
    public static async Task<bool> IsUsedShipperAsync(int shipperID)
    {
        try
        {
            return await shipperDB.IsUsedAsync(shipperID);
        }
        catch
        {
            throw;
        }
    }

    #endregion
}