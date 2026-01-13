using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBDN.Models;

namespace QLBDN.Controllers
{
    public class MatchEventsController : Controller
    {
        private readonly QlbdnContext _context;

        public MatchEventsController(QlbdnContext context)
        {
            _context = context;
        }

        // ============================================================
        // 📌 1) INDEX – List events by match
        // ============================================================
        public async Task<IActionResult> Index(int? matchId)
        {
            // Load danh sách trận (phục vụ dropdown lọc)
            ViewBag.MatchList = await _context.Matches
                .OrderByDescending(m => m.DateTime)
                .ToListAsync();

            var query = _context.MatchEvents
                .Include(e => e.Match)
                .Include(e => e.Player)
                .AsQueryable();

            if (matchId.HasValue)
            {
                ViewBag.Match = await _context.Matches
                    .FirstOrDefaultAsync(m => m.MatchId == matchId);

                query = query.Where(e => e.MatchID == matchId);
            }

            return View(await query.OrderBy(e => e.DateTime).ToListAsync());
        }


        // ============================================================
        // 📌 2) CREATE – GET
        // ============================================================
        public async Task<IActionResult> Create(int? matchId)
        {
            await LoadMatchDropdown();
            await LoadPlayersForMatch(matchId);
            LoadEventTypes();

            var model = new MatchEvent
            {
                MatchID = matchId ?? 0,
                DateTime = DateTime.Now // luôn có DateTime hợp lệ
            };

            return View(model);
        }

        // ============================================================
        // 📌 API – Lấy danh sách cầu thủ theo trận (AJAX)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetPlayersByMatch(int matchId)
        {
            var players = await _context.MatchDetails
                .Where(md => md.MatchId == matchId)
                .Select(md => md.ClubId)
                .Distinct()
                .Join(_context.Players, cid => cid, p => p.ClubId, (cid, p) => new
                {
                    p.PlayerId,
                    p.FullName
                })
                .OrderBy(p => p.FullName)
                .ToListAsync();

            return Json(players);
        }

        // ============================================================
        // 📌 3) CREATE – POST
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MatchEvent model)
        {
            LoadEventTypes(); // 🔥 BẮT BUỘC PHẢI CÓ

            if (!ModelState.IsValid)
            {
                await LoadMatchDropdown();
                await LoadPlayersForMatch(model.MatchID);

                // Debug lỗi ModelState (xem trong terminal)
                foreach (var err in ModelState)
                {
                    Console.WriteLine($"{err.Key} => {string.Join(", ", err.Value.Errors.Select(e => e.ErrorMessage))}");
                }

                return View(model);
            }

            // ---- Save Event ----
            _context.MatchEvents.Add(model);
            await _context.SaveChangesAsync();

            // ---- Update goals if applicable ----
            if (model.EventType == "Goal" ||
                model.EventType == "Penalty Goal" ||
                model.EventType == "Own Goal")
            {
                await UpdateGoalCount(model.MatchID, model.PlayerID, model.EventType);
            }

            return RedirectToAction(nameof(Index), new { matchId = model.MatchID });
        }

        // ============================================================
        // 📌 4) EDIT – GET
        // ============================================================
        public async Task<IActionResult> Edit(int id)
        {
            var e = await _context.MatchEvents.FindAsync(id);
            if (e == null) return NotFound();

            await LoadMatchDropdown();
            await LoadPlayersForMatch(e.MatchID);
            LoadEventTypes();

            return View(e);
        }

        // ============================================================
        // 📌 5) EDIT – POST
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MatchEvent model)
        {
            if (id != model.EventID) return NotFound();

            LoadEventTypes();

            if (!ModelState.IsValid)
            {
                await LoadMatchDropdown();
                await LoadPlayersForMatch(model.MatchID);
                return View(model);
            }

            _context.MatchEvents.Update(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { matchId = model.MatchID });
        }

        // ============================================================
        // 📌 6) DETAILS
        // ============================================================
        public async Task<IActionResult> Details(int id)
        {
            var evt = await _context.MatchEvents
                .Include(e => e.Match)
                .Include(e => e.Player)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (evt == null) return NotFound();

            return View(evt);
        }

        // ============================================================
        // 📌 7) DELETE – GET
        // ============================================================
        public async Task<IActionResult> Delete(int id)
        {
            var evt = await _context.MatchEvents
                .Include(e => e.Match)
                .Include(e => e.Player)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (evt == null) return NotFound();

            return View(evt);
        }

        // ============================================================
        // 📌 8) DELETE – POST
        // ============================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var evt = await _context.MatchEvents.FindAsync(id);
            if (evt == null) return NotFound();

            int matchId = evt.MatchID;

            _context.MatchEvents.Remove(evt);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { matchId });
        }

        // ============================================================
        // 📌 Load danh sách trận
        // ============================================================
        private async Task LoadMatchDropdown()
        {
            ViewBag.Matches = await _context.Matches
                .Include(m => m.MatchDetails).ThenInclude(md => md.Club)
                .OrderByDescending(m => m.MatchId)
                .ToListAsync();
        }

        // ============================================================
        // 📌 Load danh sách cầu thủ theo trận
        // ============================================================
        private async Task LoadPlayersForMatch(int? matchId)
        {
            if (!matchId.HasValue)
            {
                ViewBag.Players = new List<Player>();
                return;
            }

            ViewBag.Players = await _context.MatchDetails
                .Where(md => md.MatchId == matchId)
                .Select(md => md.ClubId)
                .Distinct()
                .Join(_context.Players, cid => cid, p => p.ClubId, (cid, p) => p)
                .OrderBy(p => p.FullName)
                .ToListAsync();
        }

        // ============================================================
        // 📌 Load danh sách loại sự kiện
        // ============================================================
        private void LoadEventTypes()
        {
            ViewBag.EventTypes = new List<string>
            {
                "Goal",
                "Own Goal",
                "Penalty Goal",
                "Penalty Miss",
                "Yellow Card",
                "Red Card",
                "Substitution",
                "VAR Review"
            };
        }

        // ============================================================
        // 📌 Auto Update Goal Count
        // ============================================================
        private async Task UpdateGoalCount(int matchId, int? playerId, string type)
        {
            if (playerId == null) return;

            var player = await _context.Players.FindAsync(playerId);
            if (player == null) return;

            var detail = await _context.MatchDetails
                .FirstOrDefaultAsync(md => md.MatchId == matchId && md.ClubId == player.ClubId);

            if (detail == null) return;

            if (type == "Own Goal")
                detail.Goals = Math.Max((detail.Goals ?? 0) - 1, 0);
            else
                detail.Goals = (detail.Goals ?? 0) + 1;

            _context.MatchDetails.Update(detail);
            await _context.SaveChangesAsync();
        }
    }
}
