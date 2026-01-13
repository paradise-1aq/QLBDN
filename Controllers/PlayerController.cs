using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBDN.Models;

namespace QLBDN.Controllers
{
    // TẤT CẢ URL bắt đầu bằng /players
    [Route("players")]
    public class PlayerController : Controller
    {
        private readonly QlbdnContext _context;
        private readonly IWebHostEnvironment _env;

        public PlayerController(QlbdnContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        /* ============================================================
         * 1) DANH SÁCH CẦU THỦ  →  GET /players?page=1
         * ============================================================ */
        [HttpGet("")]
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;

            var query = _context.Players
                .Include(p => p.Club)
                .Include(p => p.Role)
                .OrderBy(p => p.FullName);

            int total = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(total / (double)pageSize);

            var players = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalPlayers = total;

            // View: Views/Player/List.cshtml
            return View("List", players);
        }

        /* ============================================================
         * 2) CHI TIẾT CẦU THỦ  →  GET /players/5
         * ============================================================ */
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var player = await _context.Players
                .Include(p => p.Club)
                .Include(p => p.Role)
                .FirstOrDefaultAsync(p => p.PlayerId == id);

            if (player == null)
            {
                TempData["Error"] = "Không tìm thấy cầu thủ!";
                return RedirectToAction(nameof(Index));
            }

            // View: Views/Player/Details.cshtml
            return View(player);
        }

        /* ============================================================
         * 3) TẠO CẦU THỦ  →  GET /players/new
         * ============================================================ */
        [HttpGet("new")]
        public IActionResult Create()
        {
            LoadDropdowns();
            // View: Views/Player/Create.cshtml
            return View(new Player());
        }

        /* 3.1) XỬ LÝ TẠO CẦU THỦ  →  POST /players/new */
        [HttpPost("new")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Player player)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns();
                return View(player);
            }

            try
            {
                // Upload ảnh nếu có
                if (player.AvatarFile is not null && player.AvatarFile.Length > 0)
                {
                    player.AvatarUrl = await SaveImageAsync(player.AvatarFile);
                }

                _context.Players.Add(player);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm cầu thủ thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi thêm cầu thủ: {ex.Message}";
                LoadDropdowns();
                return View(player);
            }
        }

        /* ============================================================
         * 4) SỬA CẦU THỦ  →  GET /players/5/edit
         * ============================================================ */
        [HttpGet("{id:int}/edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null)
            {
                TempData["Error"] = "Không tìm thấy cầu thủ!";
                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns();
            // View: Views/Player/Edit.cshtml
            return View(player);
        }

        /* 4.1) XỬ LÝ SỬA CẦU THỦ  →  POST /players/5/edit */
        [HttpPost("{id:int}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Player player)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns();
                return View(player);
            }

            try
            {
                var existing = await _context.Players.FindAsync(id);
                if (existing == null)
                {
                    TempData["Error"] = "Không tìm thấy cầu thủ!";
                    return RedirectToAction(nameof(Index));
                }

                // Cập nhật dữ liệu
                existing.FullName    = player.FullName;
                existing.DateOfBirth = player.DateOfBirth;
                existing.Nationality = player.Nationality;
                existing.ShirtNumber = player.ShirtNumber;
                existing.RoleId      = player.RoleId;
                existing.ClubId      = player.ClubId;
                existing.Status      = player.Status;

                // Ảnh đại diện
                if (player.AvatarFile is not null && player.AvatarFile.Length > 0)
                {
                    DeleteOldImage(existing.AvatarUrl);
                    existing.AvatarUrl = await SaveImageAsync(player.AvatarFile);
                }

                _context.Update(existing);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật cầu thủ thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi cập nhật: {ex.Message}";
                LoadDropdowns();
                return View(player);
            }
        }

        /* ============================================================
         * 5) XOÁ CẦU THỦ  →  POST /players/5/delete
         * ============================================================ */
        [HttpPost("{id:int}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var player = await _context.Players.FindAsync(id);
                if (player == null)
                {
                    TempData["Error"] = "Không tìm thấy cầu thủ!";
                    return RedirectToAction(nameof(Index));
                }

                // Xoá ảnh cũ nếu có
                DeleteOldImage(player.AvatarUrl);

                _context.Players.Remove(player);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đã xoá cầu thủ!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Không thể xoá: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /* ============================================================
         * 🔧 HÀM HỖ TRỢ
         * ============================================================ */

        // Load dữ liệu dropdown cho Club & Role
        private void LoadDropdowns()
        {
            ViewBag.Clubs = _context.Clubs
                .OrderBy(c => c.Name)
                .ToList();

            ViewBag.Roles = _context.Roles
                .OrderBy(r => r.RoleName)
                .ToList();
        }

        // Lưu ảnh vào wwwroot/uploads/players và trả về URL tương đối
        private async Task<string> SaveImageAsync(IFormFile file)
        {
            string folder = Path.Combine(_env.WebRootPath, "uploads", "players");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            string filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/players/{fileName}";
        }

        // Xoá ảnh cũ nếu có
        private void DeleteOldImage(string? url)    
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            string path = Path.Combine(_env.WebRootPath, url.TrimStart('/'));
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }
    }
}
