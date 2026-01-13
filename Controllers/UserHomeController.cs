using Microsoft.AspNetCore.Mvc;
using QLBDN.Models;

public class UserHomeController : Controller
{
    private readonly QlbdnContext _context;

    public UserHomeController(QlbdnContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        ViewBag.TotalClubs = _context.Clubs.Count();
        ViewBag.TotalPlayers = _context.Players.Count();
        ViewBag.UpcomingMatches = _context.Matches
            .OrderBy(m => m.DateTime)
            .Take(5)
            .ToList();

        ViewBag.News = _context.News
            .OrderByDescending(n => n.PostedDate)
            .Take(3)
            .ToList();

        return View();
    }
}
