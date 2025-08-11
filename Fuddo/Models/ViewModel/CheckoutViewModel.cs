using Fuddo.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Fuddo.Models.ViewModel
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Họ và tên bắt buộc nhập")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Số điện thoại bắt buộc nhập")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Địa chỉ bắt buộc nhập")]
        public string Address { get; set; }

        public string? Note { get; set; }
    }
}
