using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBDN.Models;

namespace QLBDN.Controllers
{
    public class RefereesController : Controller
    {
        private readonly QlbdnContext _context;

        public RefereesController(QlbdnContext context)
        {
            _context = context;
        }

        // ============================================================
        // 📌 1) DANH SÁCH TRỌNG TÀI + LỌC THEO CẤP ĐỘ
        // ============================================================
        public async Task<IActionResult> Index(string? level)
        {
            // Load tất cả cấp độ trọng tài từ DB (không hardcode)
            ViewBag.Levels = await _context.Referees
                .Where(r => !string.IsNullOrWhiteSpace(r.Level))
                .Select(r => r.Level!.Trim())
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync();

            ViewBag.SelectedLevel = level;

            var query = _context.Referees
                .Include(r => r.RefereeMatches)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(level))
                query = query.Where(r => r.Level == level);

            return View(await query.OrderBy(r => r.FullName).ToListAsync());
        }

        // ============================================================
        // 📌 2) CHI TIẾT TRỌNG TÀI
        // ============================================================
        public async Task<IActionResult> Details(int id)
        {
            var referee = await _context.Referees
                .Include(r => r.RefereeMatches)
                    .ThenInclude(rm => rm.Match)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RefereeId == id);

            if (referee == null)
                return NotFound();

            return View(referee);
        }

        // ============================================================
        // 📌 3) TẠO MỚI TRỌNG TÀI
        // ============================================================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Referee referee)
        {
            if (!ModelState.IsValid)
                return View(referee);

            _context.Referees.Add(referee);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // 📌 4) CHỈNH SỬA TRỌNG TÀI
        // ============================================================
        public async Task<IActionResult> Edit(int id)
        {
            var referee = await _context.Referees.FindAsync(id);
            if (referee == null)
                return NotFound();

            return View(referee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Referee formModel)
        {
            var referee = await _context.Referees.FindAsync(id);
            if (referee == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View(formModel);

            referee.FullName = formModel.FullName;
            referee.DateOfBirth = formModel.DateOfBirth;
            referee.Experience = formModel.Experience;
            referee.Level = formModel.Level;
            referee.UserId = formModel.UserId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // 📌 5) XÓA TRỌNG TÀI
        // ============================================================
        public async Task<IActionResult> Delete(int id)
        {
            var referee = await _context.Referees
                .Include(r => r.RefereeMatches)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RefereeId == id);

            if (referee == null)
                return NotFound();

            return View(referee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var referee = await _context.Referees
                .Include(r => r.RefereeMatches)
                .FirstOrDefaultAsync(r => r.RefereeId == id);

            if (referee == null)
                return NotFound();

            // Xóa phân công trước → tránh lỗi FOREIGN KEY
            _context.RefereeMatches.RemoveRange(referee.RefereeMatches);
            _context.Referees.Remove(referee);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // 📌 6) PHÂN CÔNG TRỌNG TÀI CHO TRẬN (GET)
        // ============================================================
        public async Task<IActionResult> Assign(int matchId)
        {
            var match = await _context.Matches.FindAsync(matchId);
            if (match == null)
                return NotFound();

            ViewBag.Match = match;
            ViewBag.Referees = await _context.Referees.OrderBy(r => r.FullName).ToListAsync();

            var existing = await _context.RefereeMatches
                .Where(rm => rm.MatchId == matchId)
                .ToListAsync();

            return View(new AssignRefereesViewModel
            {
                MatchId = matchId,
                MainRefereeId = existing.FirstOrDefault(r => r.Role == "Main")?.RefereeId,
                Assistant1Id = existing.FirstOrDefault(r => r.Role == "Assistant 1")?.RefereeId,
                Assistant2Id = existing.FirstOrDefault(r => r.Role == "Assistant 2")?.RefereeId,
                FourthOfficialId = existing.FirstOrDefault(r => r.Role == "Fourth Official")?.RefereeId,
                VarRefereeId = existing.FirstOrDefault(r => r.Role == "VAR")?.RefereeId
            });
        }

        // ============================================================
        // 📌 7) PHÂN CÔNG TRỌNG TÀI CHO TRẬN (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(AssignRefereesViewModel model)
        {
            var match = await _context.Matches.FindAsync(model.MatchId);
            if (match == null)
                return NotFound();

            // Ngăn trùng trọng tài
            var selected = new List<int?>()
            {
                model.MainRefereeId,
                model.Assistant1Id,
                model.Assistant2Id,
                model.FourthOfficialId,
                model.VarRefereeId
            };

            if (selected.Where(x => x.HasValue)
                       .GroupBy(x => x)
                       .Any(g => g.Count() > 1))
            {
                ModelState.AddModelError("", "❌ Một trọng tài không thể giữ nhiều vai trò trong cùng một trận!");
                return View(model);
            }

            // Xóa phân công cũ
            var oldAssignments = _context.RefereeMatches.Where(rm => rm.MatchId == model.MatchId);
            _context.RefereeMatches.RemoveRange(oldAssignments);

            // Helper thêm mới
            void AssignOne(int? id, string role)
            {
                if (id.HasValue)
                {
                    _context.RefereeMatches.Add(new RefereeMatch
                    {
                        MatchId = model.MatchId,
                        RefereeId = id.Value,
                        Role = role,
                        DateTime = DateTime.Now
                    });
                }
            }

            AssignOne(model.MainRefereeId, "Main");
            AssignOne(model.Assistant1Id, "Assistant 1");
            AssignOne(model.Assistant2Id, "Assistant 2");
            AssignOne(model.FourthOfficialId, "Fourth Official");
            AssignOne(model.VarRefereeId, "VAR");

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Matches", new { id = model.MatchId });
        }
    }
}
