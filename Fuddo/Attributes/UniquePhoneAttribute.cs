using Fuddo.Service.Interface;
using System.ComponentModel.DataAnnotations;

namespace Fuddo.Attributes
{
    public class UniquePhoneAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var userService = (IUserService)validationContext.GetService(typeof(IUserService));
            var phone = value?.ToString();

            if (!string.IsNullOrEmpty(phone) && userService.GetByPhoneAsync(phone).Result != null)
            {
                return new ValidationResult("Số điện thoại đã được sử dụng.");
            }

            return ValidationResult.Success;
        }
    }

}
