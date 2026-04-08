using System.ComponentModel.DataAnnotations;

namespace SV22T1020337.Shop.Models
{
    public class CheckoutModel
    {
        [Display(Name = "Tỉnh/Thành phố giao hàng")]
        [Required(ErrorMessage = "Vui lòng nhập tỉnh/thành phố giao hàng")]
        public string DeliveryProvince { get; set; } = "";

        [Display(Name = "Địa chỉ giao hàng")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        public string DeliveryAddress { get; set; } = "";
    }
}
