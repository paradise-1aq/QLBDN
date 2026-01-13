using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBDN.Models;

namespace QLBDN.Controllers
{
    public class NewsController : Controller
    {
        private readonly QlbdnContext _context;

        public NewsController(QlbdnContext context)
        {
            _context = context;
        }

        // =====================================================================
        // 📰 1) DANH SÁCH TIN TỨC
        // =====================================================================
        public async Task<IActionResult> Index()
        {
            var news = await _context.News
                .Include(n => n.User)
                .Include(n => n.Interactions)
                .OrderByDescending(n => n.PostedDate)
                .ToListAsync();

            return View(news);
        }

        // =====================================================================
        // 📄 2) CHI TIẾT BÀI VIẾT
        // =====================================================================
        public async Task<IActionResult> Details(int id)
        {
            var article = await _context.News
                .Include(n => n.User)
                .Include(n => n.Interactions)
                    .ThenInclude(i => i.User)
                .FirstOrDefaultAsync(n => n.NewsId == id);

            if (article == null)
                return NotFound();

            return View(article);
        }

        // =====================================================================
        // ✍️ 3) TẠO BÀI VIẾT MỚI
        // =====================================================================
        public IActionResult Create()
        {
            // Nếu chưa đăng nhập → quay lại Login
            var uid = HttpContext.Session.GetInt32("UserId");
            if (uid == null)
                return RedirectToAction("Login", "User");

            return View(new News
            {
                PostedDate = DateTime.Now
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(News news)
        {
            var uid = HttpContext.Session.GetInt32("UserId");
            if (uid == null)
                return RedirectToAction("Login", "User");

            if (!ModelState.IsValid)
                return View(news);

            // Nếu không có ảnh → dùng ảnh mặc định
            if (string.IsNullOrEmpty(news.ImageUrl))
                news.ImageUrl = "/images/default-news.jpg";

            news.UserId = uid.Value;          // ⭐ LẤY TỪ SESSION
            news.PostedDate = DateTime.Now;

            _context.News.Add(news);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =====================================================================
        // ✏️ 4) CHỈNH SỬA BÀI VIẾT
        // =====================================================================
        public async Task<IActionResult> Edit(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
                return NotFound();

            return View(news);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, News form)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View(form);

            news.Title = form.Title;
            news.Content = form.Content;
            news.ImageUrl = string.IsNullOrEmpty(form.ImageUrl)
                            ? "/images/default-news.jpg"
                            : form.ImageUrl;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =====================================================================
        // 🗑 5) XOÁ BÀI VIẾT
        // =====================================================================
        public async Task<IActionResult> Delete(int id)
        {
            var news = await _context.News
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.NewsId == id);

            if (news == null)
                return NotFound();

            return View(news);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var news = await _context.News
                .Include(n => n.Interactions)
                .FirstOrDefaultAsync(n => n.NewsId == id);

            if (news == null)
                return NotFound();

            // Xóa comment trước
            if (news.Interactions.Any())
                _context.Interactions.RemoveRange(news.Interactions);

            _context.News.Remove(news);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
