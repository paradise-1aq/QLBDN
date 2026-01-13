using Microsoft.AspNetCore.Mvc;

namespace QLBDN.Controllers
{
    public class SettingsController : Controller
    {
        // GET: /Settings
        public IActionResult Index()
        {
            return View();
        }

        // Ví dụ form đổi mật khẩu (POST)
        [HttpPost]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            // TODO: xử lý đổi mật khẩu thật sự (check user hiện tại, hash, lưu DB...)
            // Tạm thời chỉ hiển thị thông báo
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword != confirmPassword)
            {
                ViewData["Message"] = "Mật khẩu mới không hợp lệ.";
            }
            else
            {
                ViewData["Message"] = "Đã giả lập đổi mật khẩu thành công (bạn tự cài đặt logic thật).";
            }

            return View("Index");
        }
    }
}
