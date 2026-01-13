using System.ComponentModel.DataAnnotations;

namespace QLBDN.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MaxLength(50)]
        public string Password { get; set; } = string.Empty;
    }
}
