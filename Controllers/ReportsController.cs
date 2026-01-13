using Microsoft.AspNetCore.Mvc;
using QLBDN.Models;
using System.Linq;

namespace QLBDN.Controllers
{
    public class ReportsController : Controller
    {
        private readonly QlbdnContext _context;

        public ReportsController(QlbdnContext context)
        {
            _context = context;
        }

        // GET: /Reports
        public IActionResult Index()
        {
            /* =======================
             * 1️⃣ KPI TỔNG QUAN
             * ======================= */

            ViewData["TotalClubs"] = _context.Clubs.Count();
            ViewData["TotalPlayers"] = _context.Players.Count();
            ViewData["TotalMatches"] = _context.Matches.Count();

            // Tổng bàn thắng (từ MATCH_DETAIL)
            ViewData["TotalGoals"] = _context.MatchDetails
                .Sum(md => md.Goals ?? 0);

            /* =======================
             * 2️⃣ BIỂU ĐỒ THEO MÙA GIẢI
             * ======================= */

            var seasonChartData = _context.Matches
                .Where(m => m.Season != null)
                .Select(m => new
                {
                    SeasonName = m.Season!.Name,
                    Goals = m.MatchDetails.Sum(md => md.Goals ?? 0)
                })
                .GroupBy(x => x.SeasonName)
                .Select(g => new
                {
                    Season = g.Key,
                    Matches = g.Count(),
                    Goals = g.Sum(x => x.Goals)
                })
                .OrderBy(x => x.Season)
                .ToList();

            ViewData["ChartLabels"] = seasonChartData.Select(x => x.Season).ToList();
            ViewData["MatchesData"] = seasonChartData.Select(x => x.Matches).ToList();
            ViewData["GoalsData"] = seasonChartData.Select(x => x.Goals).ToList();

            /* =======================
             * 3️⃣ THỐNG KÊ CÂU LẠC BỘ
             * ======================= */

            // TOP 5 CLB ghi nhiều bàn nhất
            var topClubsByGoals = _context.MatchDetails
                .GroupBy(md => new { md.ClubId, md.Club!.Name })
                .Select(g => new
                {
                    ClubName = g.Key.Name,
                    Goals = g.Sum(x => x.Goals ?? 0)
                })
                .OrderByDescending(x => x.Goals)
                .Take(5)
                .ToList();

            ViewData["TopClubNames"] = topClubsByGoals.Select(x => x.ClubName).ToList();
            ViewData["TopClubGoals"] = topClubsByGoals.Select(x => x.Goals).ToList();

            // CLB thắng nhiều trận nhất
            var topClubByWins = _context.MatchDetails
                .Where(md => md.IsWinner == true)
                .GroupBy(md => new { md.ClubId, md.Club!.Name })
                .Select(g => new
                {
                    ClubName = g.Key.Name,
                    Wins = g.Count()
                })
                .OrderByDescending(x => x.Wins)
                .FirstOrDefault();

            ViewData["BestClubName"] = topClubByWins?.ClubName ?? "—";
            ViewData["BestClubWins"] = topClubByWins?.Wins ?? 0;
            /* =======================
            * 4️⃣ THỐNG KÊ CẦU THỦ
            * ======================= */

            // TOP 5 CẦU THỦ GHI BÀN
            var topPlayersByGoals = _context.MatchEvents
                .Where(me => me.EventType == "Goal" && me.Player != null)
                .GroupBy(me => new { me.PlayerID, me.Player!.FullName })
                .Select(g => new
                {
                    PlayerName = g.Key.FullName,
                    Goals = g.Count()
                })
                .OrderByDescending(x => x.Goals)
                .Take(5)
                .ToList();

            ViewData["TopPlayerNames"] = topPlayersByGoals.Select(x => x.PlayerName).ToList();
            ViewData["TopPlayerGoals"] = topPlayersByGoals.Select(x => x.Goals).ToList();

            // CẦU THỦ GHI BÀN NHIỀU NHẤT
            var bestScorer = topPlayersByGoals.FirstOrDefault();

            ViewData["BestPlayerName"] = bestScorer?.PlayerName ?? "—";
            ViewData["BestPlayerGoals"] = bestScorer?.Goals ?? 0;

            // CẦU THỦ NHẬN NHIỀU THẺ NHẤT
            var mostCardsPlayer = _context.MatchEvents
                .Where(me =>
                    me.Player != null &&
                    (me.EventType == "Yellow Card" || me.EventType == "Red Card"))
                .GroupBy(me => new { me.PlayerID, me.Player!.FullName })
                .Select(g => new
                {
                    PlayerName = g.Key.FullName,
                    Cards = g.Count()
                })
                .OrderByDescending(x => x.Cards)
                .FirstOrDefault();

            ViewData["MostCardPlayerName"] = mostCardsPlayer?.PlayerName ?? "—";
            ViewData["MostCardCount"] = mostCardsPlayer?.Cards ?? 0;

            return View();
        }
        
    }
}
