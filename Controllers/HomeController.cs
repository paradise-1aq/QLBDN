using Microsoft.AspNetCore.Mvc;
using QLBDN.Models;
using Microsoft.EntityFrameworkCore;

namespace QLBDN.Controllers
{
    public class HomeController : Controller
    {
        private readonly QlbdnContext _context;

        public HomeController(QlbdnContext context)
        {
            _context = context;
        }

        // GET: /
        public async Task<IActionResult> Index()
        {
            ViewBag.ClubCount = await _context.Clubs.CountAsync();
            ViewBag.PlayerCount = await _context.Players.CountAsync();
            ViewBag.MatchCount = await _context.Matches.CountAsync();
            ViewBag.RefereeCount = await _context.Referees.CountAsync();
            ViewBag.EventCount = await _context.MatchEvents.CountAsync();
            ViewBag.TicketCount = await _context.TicketBookings.CountAsync();

            return View();
        }
    }
}
