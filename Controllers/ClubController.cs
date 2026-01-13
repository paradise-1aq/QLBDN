using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBDN.Models;

namespace QLBDN.Controllers
{
    public class ClubController : Controller
    {
        private readonly QlbdnContext _context;

        public ClubController(QlbdnContext context)
        {
            _context = context;
        }

        // ==========================
        // 🟢 INDEX + FILTER
        // ==========================
        public async Task<IActionResult> Index(string? table)
        {
            ViewBag.Table = table;

            var clubsQuery = _context.Clubs
                .Include(c => c.Players)
                .AsQueryable();

            if (!string.IsNullOrEmpty(table))
            {
                clubsQuery = clubsQuery.Where(c =>
                    _context.MatchDetails
                        .Include(md => md.Match)
                        .ThenInclude(m => m.Round)
                        .Any(md =>
                            md.ClubId == c.ClubId &&
                            md.Match.Round != null &&
                            md.Match.Round.TableName == table));
            }

            var clubs = await clubsQuery.OrderBy(c => c.ClubId).ToListAsync();
            return View(clubs);
        }

        // ==========================
        // 🟢 DETAILS
        // ==========================
        public async Task<IActionResult> Detail(int id)
        {
            var club = await _context.Clubs.FindAsync(id);
            if (club == null) return NotFound();

            var players = await _context.Players
                .Include(p => p.Role)
                .Where(p => p.ClubId == id)
                .ToListAsync();

            ViewData["Club"] = club;
            ViewData["Players"] = players;

            return View("ClubDetail");
        }

        // ==========================
        // 🟢 CREATE
        // ==========================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Club club)
        {
            if (!ModelState.IsValid)
                return View(club);

            _context.Clubs.Add(club);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ==========================
        // 🟡 EDIT
        // ==========================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var club = await _context.Clubs.FindAsync(id);
            if (club == null) return NotFound();

            return View(club);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Club club)
        {
            if (id != club.ClubId)
                return BadRequest();

            _context.Update(club);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ==========================
        // 🔴 DELETE
        // ==========================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var club = await _context.Clubs.FindAsync(id);
            if (club == null) return NotFound();

            _context.Clubs.Remove(club);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
