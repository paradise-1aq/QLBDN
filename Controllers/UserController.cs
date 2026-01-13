using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBDN.Models;

namespace QLBDN.Controllers
{
    public class UserController : Controller
    {
        private readonly QlbdnContext _context;

        public UserController(QlbdnContext context)
        {
            _context = context;
        }

        // ====================== LOGIN ======================
        public IActionResult Login()
        {
            return View(new LoginModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);

            if (user == null)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
                return View(model);
            }

            // Lưu session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            // Điều hướng theo Role
            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Index", "UserHome");
            }
        }


        // ====================== REGISTER ======================
        public IActionResult Register()
        {
            return View(new RegisterModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại!";
                return View(model);
            }

            var newUser = new User
            {
                Username = model.Username,
                Password = model.Password,
                Role = "User"
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        // ====================== LOGOUT ======================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
