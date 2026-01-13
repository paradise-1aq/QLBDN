using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBDN.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

public class TicketBookingController : Controller
{
    private readonly QlbdnContext _context;

    public TicketBookingController(QlbdnContext context)
    {
        _context = context;
    }

    // ============================================================
    // 1) CHỌN VÒNG ĐẤU
    // ============================================================
    public async Task<IActionResult> SelectRound()
    {
        var rounds = await _context.Rounds
            .OrderBy(r => r.RoundId)
            .ToListAsync();

        return View(rounds);
    }

    // ============================================================
    // 2) CHỌN TRẬN THEO VÒNG
    // ============================================================
    public async Task<IActionResult> SelectMatchByRound(int roundId)
    {
        if (roundId <= 0)
            return BadRequest("Round ID không hợp lệ.");

        var matches = await _context.Matches
            .Where(m => m.RoundId == roundId)
            .Include(m => m.MatchDetails)
                .ThenInclude(md => md.Club)
            .OrderBy(m => m.DateTime)
            .ToListAsync();

        var result = new List<dynamic>();

        foreach (var match in matches)
        {
            if (match.MatchDetails == null || match.MatchDetails.Count < 2)
                continue;

            var details = match.MatchDetails.ToList();

            var home = details.FirstOrDefault(x => x.IsHomeTeam == true);
            var away = details.FirstOrDefault(x => x.IsHomeTeam == false);

            if (home == null || away == null)
            {
                home = details[0];
                away = details[1];
            }

            result.Add(new
            {
                MatchId = match.MatchId,
                Match = match,
                HomeClub = home.Club,
                AwayClub = away.Club
            });
        }

        return View("SelectMatchByRound", result);
    }

    // ============================================================
    // 3) MUA VÉ → NHẬP SỐ LƯỢNG
    // ============================================================
    public async Task<IActionResult> Buy(int matchId)
    {
        var match = await _context.Matches
            .Include(m => m.Round)
            .Include(m => m.Season)
            .FirstOrDefaultAsync(m => m.MatchId == matchId);

        if (match == null)
            return NotFound();

        return View(match);
    }

    [HttpPost]
    public IActionResult Buy(int matchId, int quantity)
    {
        if (quantity <= 0)
        {
            TempData["Error"] = "Số lượng phải lớn hơn 0.";
            return RedirectToAction("Buy", new { matchId });
        }

        decimal price = 100000;
        decimal total = quantity * price;

        return RedirectToAction("Payment", new { matchId, quantity, total });
    }

    // ============================================================
    // 4) TRANG THANH TOÁN
    // ============================================================
    public async Task<IActionResult> Payment(int matchId, int quantity, decimal total)
    {
        var match = await _context.Matches
            .Include(m => m.Round)
            .Include(m => m.Season)
            .FirstOrDefaultAsync(m => m.MatchId == matchId);

        if (match == null)
            return NotFound();

        ViewBag.Quantity = quantity;
        ViewBag.Total = total;

        return View(match);
    }

    // ============================================================
    // 5) XÁC NHẬN THANH TOÁN → TRẢ FILE PDF VỀ MÁY
    // ============================================================
   [HttpPost]
    public async Task<IActionResult> ConfirmPayment(
        int matchId,
        int quantity,
        decimal total,
        string fullname,
        string phone,
        string email)
    {
        if (matchId <= 0 || quantity <= 0)
            return BadRequest("Dữ liệu không hợp lệ.");

        // 1) Lưu booking
        var booking = new TicketBooking
        {
            MatchId = matchId,
            UserId = 1,
            Quantity = quantity,
            BookingDateTime = DateTime.Now,
            TotalPrice = total,
            Status = "Paid"
        };

        _context.TicketBookings.Add(booking);
        await _context.SaveChangesAsync();

        // 2) Tạo PDF
        string pdfPath = GeneratePdf(booking, fullname, email);

        // → Convert thành đường dẫn URL để trình duyệt tải
        string relativePath = "/tickets/" + Path.GetFileName(pdfPath);

        // 3) Truyền sang Success
        TempData["PdfPath"] = relativePath;
        TempData["BookingId"] = booking.BookingId;
        TempData["BuyerName"] = fullname;
        TempData["BuyerEmail"] = email;

        return RedirectToAction("Success");
    }


    // ============================================================
    // 🔥 6) TẠO PDF VÉ
    // ============================================================
   // Hàm vẽ bo góc 
private void DrawRoundedRect(XGraphics gfx, XPen pen, XBrush brush,
    double x, double y, double width, double height, double radius)
{
    var path = new XGraphicsPath();

    path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
    path.AddArc(x + width - radius * 2, y, radius * 2, radius * 2, 270, 90);
    path.AddArc(x + width - radius * 2, y + height - radius * 2, radius * 2, radius * 2, 0, 90);
    path.AddArc(x, y + height - radius * 2, radius * 2, radius * 2, 90, 90);

    path.CloseFigure();

    gfx.DrawPath(pen, brush, path);
}


private string GeneratePdf(TicketBooking booking, string fullname, string email)
{
    string folder = Path.Combine("wwwroot", "tickets");
    if (!Directory.Exists(folder))
        Directory.CreateDirectory(folder);

    string path = Path.Combine(folder, $"ticket_{booking.BookingId}.pdf");

    // Lấy thông tin trận đấu
    var match = _context.Matches
        .Include(m => m.MatchDetails).ThenInclude(md => md.Club)
        .FirstOrDefault(m => m.MatchId == booking.MatchId);

    var home = match.MatchDetails.FirstOrDefault(x => x.IsHomeTeam == true)?.Club;
    var away = match.MatchDetails.FirstOrDefault(x => x.IsHomeTeam == false)?.Club;

    // Tạo PDF
    PdfDocument doc = new PdfDocument();
    PdfPage page = doc.AddPage();
    page.Width = 600;
    page.Height = 350;

    XGraphics gfx = XGraphics.FromPdfPage(page);

    // Màu gradient Premier League
    XColor purple = XColor.FromArgb(102, 0, 153);
    XColor pink = XColor.FromArgb(234, 0, 94);

    var gradient = new XLinearGradientBrush(
        new XPoint(0, 0),
        new XPoint(page.Width, page.Height),
        purple,
        pink
    );

    gfx.DrawRectangle(gradient, 0, 0, page.Width, page.Height);

    // Fonts
    XFont titleFont = new XFont("Arial", 28, XFontStyle.Bold);
    XFont bigFont = new XFont("Arial", 20, XFontStyle.Bold);
    XFont labelFont = new XFont("Arial", 14, XFontStyle.Bold);

    // ===== TIÊU ĐỀ =====
    gfx.DrawString("Giải Bóng Đá Nam", titleFont, XBrushes.White,
        new XRect(0, 20, page.Width, 40), XStringFormats.TopCenter);

    // ===== KHUNG TRẬN ĐẤU =====
    var matchBox = new XRect(40, 80, 520, 80);
    gfx.DrawRoundedRectangle(new XPen(XColors.White, 2), XBrushes.White, matchBox, new XSize(20, 20));

    gfx.DrawString(home?.Name ?? "Home", bigFont, XBrushes.Black,
        new XRect(matchBox.X + 10, matchBox.Y + 20, 200, 40), XStringFormats.CenterLeft);

    gfx.DrawString("VS", bigFont, XBrushes.Black,
        new XRect(0, matchBox.Y + 20, page.Width, 40), XStringFormats.Center);

    gfx.DrawString(away?.Name ?? "Away", bigFont, XBrushes.Black,
        new XRect(matchBox.Right - 210, matchBox.Y + 20, 200, 40), XStringFormats.CenterRight);

    // ===== KHUNG CHI TIẾT =====
    var infoBox = new XRect(40, 180, 520, 110);
    gfx.DrawRoundedRectangle(
        new XPen(XColors.White, 2),
        new XSolidBrush(XColor.FromArgb(255, 245, 245, 245)),
        infoBox,
        new XSize(20, 20)
    );

    gfx.DrawString($"Sân: {match.Stadium}", labelFont, XBrushes.Black,
        new XRect(infoBox.X + 15, infoBox.Y + 10, 350, 30), XStringFormats.TopLeft);

    gfx.DrawString($"Ngày: {match.DateTime:dd/MM/yyyy HH:mm}", labelFont, XBrushes.Black,
        new XRect(infoBox.X + 15, infoBox.Y + 40, 350, 30), XStringFormats.TopLeft);

    gfx.DrawString($"Khách: {fullname}", labelFont, XBrushes.Black,
        new XRect(infoBox.X + 15, infoBox.Y + 70, 350, 30), XStringFormats.TopLeft);

    // ===== QR CODE (CĂN CHUẨN KHÔNG LỆCH) =====
    string qrData = $"QLBDN-TICKET-{booking.BookingId}";
    string qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=150x150&data={qrData}";
    string qrPath = Path.Combine(folder, $"qr_{booking.BookingId}.png");

    using (var client = new HttpClient())
    {
        var img = client.GetByteArrayAsync(qrUrl).Result;
        System.IO.File.WriteAllBytes(qrPath, img);
    }

    XImage qrImg = XImage.FromFile(qrPath);

    int qrSize = 100;

    double qrX = infoBox.Right - qrSize - 20; // cách phải 20px
    double qrY = infoBox.Y + (infoBox.Height - qrSize) / 2; // căn giữa dọc

    gfx.DrawImage(qrImg, qrX, qrY, qrSize, qrSize);

    // Mã vé đặt cạnh QR, căn giữa dọc
    gfx.DrawString($"Mã vé: {booking.BookingId}", labelFont, XBrushes.Black,
        new XRect(qrX - 130, qrY + 40, 120, 30), XStringFormats.CenterRight);

    System.IO.File.Delete(qrPath);

    // Lưu PDF
    doc.Save(path);
    return path;
}



    // ============================================================
    // 7) SUCCESS PAGE (nếu cần sử dụng)
    // ============================================================
    public IActionResult Success()
    {
        return View();
    }
}
