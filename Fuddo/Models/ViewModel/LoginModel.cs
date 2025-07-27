﻿using System.ComponentModel.DataAnnotations;

namespace Fuddo.Models.ViewModel
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        public string Password { get; set; } = string.Empty;
    }
}
