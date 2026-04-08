using System.ComponentModel.DataAnnotations;

namespace SV22T1020337.Shop.Models
{
    public class ProfileModel
    {
        public int CustomerID { get; set; }

        [Display(Name = "Tên khách hàng")]
        [Required(ErrorMessage = "Vui lòng nhập tên khách hàng")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên khách hàng từ 3 đến 100 ký tự")]
        public string CustomerName { get; set; } = "";

        [Display(Name = "Tên giao dịch")]
        [Required(ErrorMessage = "Vui lòng nhập tên giao dịch")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên giao dịch từ 3 đến 100 ký tự")]
        public string ContactName { get; set; } = "";

        [Display(Name = "Tỉnh/Thành")]
        [Required(ErrorMessage = "Vui lòng nhập tỉnh/thành")]
        [StringLength(100, ErrorMessage = "Tỉnh/thành không quá 100 ký tự")]
        public string Province { get; set; } = "";

        [Display(Name = "Địa chỉ")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [StringLength(200, ErrorMessage = "Địa chỉ không quá 200 ký tự")]
        public string Address { get; set; } = "";

        [Display(Name = "Điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^(03|05|07|08|09)\d{8}$",
            ErrorMessage = "Số điện thoại không hợp lệ (10 số, đúng đầu số VN)")]
        public string Phone { get; set; } = "";

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không quá 100 ký tự")]
        public string Email { get; set; } = "";
    }
}