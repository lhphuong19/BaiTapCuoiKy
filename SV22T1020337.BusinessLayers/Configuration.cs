namespace SV22T1020337.BusinessLayers
{
    /// <summary>
    /// Lớp lưu giữ thông tin cấu hình cần sử dụng cho BussinessLayers
    /// </summary>
    public static class Configuration
    {
        private static string _connectionString = "";

        /// <summary>
        /// Khởi tạo cấu hình cho BusinessLayers
        /// (Hàm này phải được gọi trước khi chạy ứng dụng)
        /// </summary>
        /// <param name="connectionString"></param>
        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Thuộc tính trả về chuỗi tham số kết nối đến cơ sở dữ liệu
        /// </summary>
        public static string ConnectionString => _connectionString;
    }
}
