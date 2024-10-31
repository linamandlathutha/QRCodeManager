using Microsoft.AspNetCore.Mvc;
using ZXing;
using ZXing.QrCode;
using System.Drawing;
using Microsoft.AspNetCore.Identity;
using QRCodeManager.Models;
using System.Drawing.Imaging;
using Microsoft.EntityFrameworkCore;
using QRCodeManager.Data;

public class QRCodeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public QRCodeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Generate QR Code
    public IActionResult Generate() => View();

    [HttpPost]
    public async Task<IActionResult> Generate(string text)
    {
        if (string.IsNullOrEmpty(text)) return View();

        var qrCodeWriter = new QRCodeWriter();
        var qrCode = qrCodeWriter.encode(text, BarcodeFormat.QR_CODE, 250, 250);

        // Ensure the directory exists
        var imagePath = Path.Combine("wwwroot", "images", "qrcodes");
        if (!Directory.Exists(imagePath))
        {
            Directory.CreateDirectory(imagePath);
        }

        var fileName = $"{Guid.NewGuid()}.png";
        var fullPath = Path.Combine(imagePath, fileName);

        using (var bitmap = new Bitmap(qrCode.Width, qrCode.Height))
        {
            for (int y = 0; y < qrCode.Height; y++)
            {
                for (int x = 0; x < qrCode.Width; x++)
                {
                    bitmap.SetPixel(x, y, qrCode[x, y] ? Color.Black : Color.White);
                }
            }

            bitmap.Save(fullPath, ImageFormat.Png);

            var qrCodeEntry = new QRCodeModel
            {
                UserId = _userManager.GetUserId(User),
                Data = text,
                ImageUrl = $"/images/qrcodes/{fileName}", // URL relative to wwwroot
                CreatedAt = DateTime.Now
            };

            _context.QRCodes.Add(qrCodeEntry);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("History");
    }


    // GET: QR Code History
    public async Task<IActionResult> History()
    {
        var userId = _userManager.GetUserId(User);
        var history = await _context.QRCodes
            .Where(q => q.UserId == userId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
        return View(history);
    }
}
