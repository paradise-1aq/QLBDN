using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBDN.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QLBDN.Controllers
{
    public class StandingsController : Controller
    {
        private readonly QlbdnContext _context;

        public StandingsController(QlbdnContext context)
        {
            _context = context;
        }

        // URL mặc định: /Standings hoặc /Standings/Index
        public async Task<IActionResult> Index(int? seasonId)
        {
            // --- ĐOẠN TÍNH TOÁN BXH: dùng lại logic bạn đã có ---
            var season = seasonId.HasValue
                ? await _context.Seasons.FindAsync(seasonId.Value)
                : await _context.Seasons
                    .OrderByDescending(s => s.StartDate ?? DateTime.MinValue)
                    .FirstOrDefaultAsync();

            if (season == null)
                return NotFound("Không tìm thấy mùa giải.");

            var clubs = await _context.Clubs.OrderBy(c => c.Name).ToListAsync();
            var table = clubs.ToDictionary(
                c => c.ClubId,
                c => new StandingsRowViewModel
                {
                    ClubId = c.ClubId,
                    ClubName = c.Name
                });

            var matches = await _context.Matches
                .Include(m => m.MatchDetails)
                .Where(m => m.SeasonId == season.SeasonId)
                .ToListAsync();

            foreach (var match in matches)
            {
                if (match.MatchDetails == null) continue;
                var details = match.MatchDetails.ToList();
                if (details.Count < 2) continue;

                var home = details.First();
                var away = details[1];

                if (home.Goals == null || away.Goals == null) continue;

                if (!table.TryGetValue(home.ClubId, out var h) ||
                    !table.TryGetValue(away.ClubId, out var a)) continue;

                int hg = home.Goals ?? 0;
                int ag = away.Goals ?? 0;

                h.Played++; a.Played++;
                h.GoalsFor += hg; h.GoalsAgainst += ag;
                a.GoalsFor += ag; a.GoalsAgainst += hg;

                if (hg > ag) { h.Won++; a.Lost++; }
                else if (hg < ag) { a.Won++; h.Lost++; }
                else { h.Drawn++; a.Drawn++; }
            }

            var list = table.Values
                .OrderByDescending(r => r.Points)
                .ThenByDescending(r => r.GoalDifference)
                .ThenByDescending(r => r.GoalsFor)
                .ThenBy(r => r.ClubName)
                .ToList();

            int rank = 1;
            foreach (var row in list) row.Rank = rank++;

            ViewData["Season"] = season;
            ViewData["Seasons"] = await _context.Seasons
                .OrderByDescending(s => s.StartDate ?? DateTime.MinValue)
                .ToListAsync();

            return View("Index", list);   // Views/Standings/Index.cshtml
        }
    }
}
