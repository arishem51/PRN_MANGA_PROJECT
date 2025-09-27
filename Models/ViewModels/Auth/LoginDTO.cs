using System.ComponentModel.DataAnnotations;

namespace PRN_MANGA_PROJECT.Models.ViewModels.Auth
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [Display(Name = "Username")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; }
    }


}
