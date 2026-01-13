using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLBDN.Models;

namespace QLBDN.Controllers
{
    public class MatchesController : Controller
    {
        private readonly QlbdnContext _context;

        public MatchesController(QlbdnContext context)
        {
            _context = context;
        }

        // ============================================================
        // 🟢 1) INDEX — ĐÃ CẬP NHẬT SORTING THEO LOẠI VÒNG
        // ============================================================
        public async Task<IActionResult> Index(int? seasonId)
        {
            var seasonList = await _context.Seasons
                .OrderByDescending(s => s.StartDate ?? DateTime.MinValue)
                .ThenByDescending(s => s.SeasonId)
                .ToListAsync();

            if (!seasonId.HasValue && seasonList.Count > 0)
            {
                seasonId = seasonList.First().SeasonId;
            }

            ViewBag.Seasons = seasonList
                .Select(s => new SelectListItem
                {
                    Value = s.SeasonId.ToString(),
                    Text = s.Name,
                    Selected = seasonId.HasValue && s.SeasonId == seasonId.Value
                })
                .ToList();

            ViewBag.SelectedSeasonId = seasonId;

            var query = _context.Matches
                .Include(m => m.Round)
                .Include(m => m.MatchDetails)
                    .ThenInclude(md => md.Club)
                .AsQueryable();

            if (seasonId.HasValue)
            {
                query = query.Where(m => m.SeasonId == seasonId.Value);
            }

            var matches = await query.ToListAsync();

            // ========================================================
            // 🟢 SORTING THEO LOGIC CHUYÊN NGHIỆP
            // ========================================================
            matches = matches
                .OrderBy(m =>
                    m.Round?.RoundName switch
                    {
                        "Vòng bảng" => 1,
                        "Bán kết" => 2,
                        "Chung kết" => 3,
                        _ => 99
                    }
                )
                .ThenBy(m => m.Round?.TableName) // A trước B
                .ThenBy(m => m.MatchId)
                .ToList();

            return View(matches);
        }

        // ============================================================
        // 🟢 2) CREATE — GET
        // ============================================================
        [HttpGet]
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        // ============================================================
        // 🟢 3) CREATE — POST
        // ============================================================
        [HttpPost]
    public async Task<IActionResult> Create(
        int homeClubId,
        int awayClubId,
        int seasonId,
        int roundId,
        DateTime dateTime,
        string stadium,
        string? tableName // 🟢 NHẬN BẢNG A/B
    )
    {
        if (homeClubId == awayClubId)
        {
            TempData["Error"] = "Hai đội không được trùng nhau!";
            LoadDropdowns();
            return View();
        }

        var round = await _context.Rounds.FindAsync(roundId);

        // Nếu là vòng bảng → cập nhật bảng A/B
        if (round.RoundName == "Vòng bảng" && !string.IsNullOrEmpty(tableName))
        {
            round.TableName = tableName;   // A hoặc B
            _context.Rounds.Update(round);
        }

        var match = new Match
        {
            Stadium = stadium,
            DateTime = dateTime,
            SeasonId = seasonId,
            RoundId = roundId,
            Status = "Scheduled"
        };

        _context.Matches.Add(match);
        await _context.SaveChangesAsync();

        _context.MatchDetails.AddRange(
            new MatchDetail
            {
                MatchId = match.MatchId,
                ClubId = homeClubId,
                IsHomeTeam = true,
                Goals = 0
            },
            new MatchDetail
            {
                MatchId = match.MatchId,
                ClubId = awayClubId,
                IsHomeTeam = false,
                Goals = 0
            }
        );

        await _context.SaveChangesAsync();

        TempData["Success"] = "Tạo trận đấu thành công!";
        return RedirectToAction(nameof(Index));
    }

        // ============================================================
        // 🟢 4) DETAILS
        // ============================================================
        public async Task<IActionResult> Details(int id)
        {
            var match = await _context.Matches
                .Include(m => m.Round)
                .Include(m => m.Season)
                .Include(m => m.MatchDetails).ThenInclude(md => md.Club)
                .FirstOrDefaultAsync(m => m.MatchId == id);

            if (match == null)
                return NotFound();

            return View(match);
        }

        // ============================================================
        // 🟢 5) EDIT — GET
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var match = await _context.Matches
                .Include(m => m.MatchDetails)
                .FirstOrDefaultAsync(m => m.MatchId == id);

            if (match == null)
                return NotFound();

            LoadDropdowns();

            var home = match.MatchDetails.First(md => md.IsHomeTeam == true);
            var away = match.MatchDetails.First(md => md.IsHomeTeam == false);

            ViewBag.HomeId = home.ClubId;
            ViewBag.AwayId = away.ClubId;

            return View(match);
        }

        // ============================================================
        // 🟢 6) EDIT — POST
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Edit(
            int id,
            int homeClubId,
            int awayClubId,
            int seasonId,
            int roundId,
            DateTime dateTime,
            string stadium,
            int homeGoals,
            int awayGoals)
        {
            var match = await _context.Matches
                .Include(m => m.MatchDetails)
                .FirstOrDefaultAsync(m => m.MatchId == id);

            if (match == null)
                return NotFound();

            if (homeClubId == awayClubId)
            {
                TempData["Error"] = "Hai đội không thể giống nhau!";
                LoadDropdowns();
                return View(match);
            }

            // cập nhật thông tin chung
            match.SeasonId = seasonId;
            match.RoundId  = roundId;
            match.DateTime = dateTime;
            match.Stadium  = stadium;

            // ✅ dùng so sánh == true / == false vì IsHomeTeam là bool?
            var home = match.MatchDetails.First(md => md.IsHomeTeam == true);
            var away = match.MatchDetails.First(md => md.IsHomeTeam == false); // hoặc md.IsHomeTeam != true

            home.ClubId = homeClubId;
            away.ClubId = awayClubId;

            // 🔢 cập nhật tỷ số
            home.Goals = homeGoals;
            away.Goals = awayGoals;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật trận đấu thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // 🟢 7) DELETE
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var match = await _context.Matches
                .Include(m => m.MatchDetails)
                .FirstOrDefaultAsync(m => m.MatchId == id);

            if (match == null)
                return NotFound();

            // XÓA MATCH_DETAIL TRƯỚC
            if (match.MatchDetails != null && match.MatchDetails.Any())
            {
                _context.MatchDetails.RemoveRange(match.MatchDetails);
            }

            // Sau đó xoá MATCH
            _context.Matches.Remove(match);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xoá trận đấu!";
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // 🟢 8) LOAD DROPDOWNS
        // ============================================================
        private void LoadDropdowns()
        {
            ViewBag.Clubs = _context.Clubs.OrderBy(c => c.Name).ToList();
            ViewBag.Rounds = _context.Rounds.OrderBy(r => r.RoundId).ToList();
            ViewBag.Seasons = _context.Seasons.OrderByDescending(s => s.StartDate).ToList();
        }
    }
}
