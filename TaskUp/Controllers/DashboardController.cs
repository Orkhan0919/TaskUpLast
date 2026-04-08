using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskUp.Data;
using TaskUp.Models;
using TaskUp.ViewModels;
using TaskUp.Utilities.Enums;

namespace TaskUp.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(AppDbContext context, UserManager<AppUser> userManager,
        ILogger<DashboardController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public class CreateBoardRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? BoardType { get; set; }
        public bool EnablePassword { get; set; }
        public string? Password { get; set; }
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var boards = await _context.Boards
            .Include(b => b.Members)
            .Include(b => b.Columns)
            .ThenInclude(c => c.Tasks)
            .Where(b => b.OwnerId == user.Id ||
                        b.Members.Any(m => m.UserId == user.Id))
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return View(boards);
    }

    [HttpGet]
    public IActionResult Access()
    {
        var model = new DashboardAccessVm
        {
            ShowDemoInfo = true,
            RequiresPassword = false 
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBoard([FromBody] CreateBoardRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Json(new { success = false, message = "Board name is required" });

            if (request.EnablePassword)
            {
                if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 4)
                    return Json(new { success = false, message = "Password must be at least 4 characters" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "User not found" });

            string uniqueCode;
            do
            {
                uniqueCode = GenerateRandomCode(6);
            } 
            while (await _context.Boards.AnyAsync(b => b.JoinCode == uniqueCode));

            BoardType boardType = request.BoardType == "personal" ? BoardType.Personal : BoardType.Team;

            var newBoard = new Board
            {
                Name = request.Name,
                Description = request.Description ?? "",
                OwnerId = user.Id,
                JoinCode = uniqueCode,
                BoardType = boardType,
                IsPrivate = request.EnablePassword, 
                Password = request.EnablePassword ? request.Password : null,  
                CreatedAt = DateTime.Now
            };

            _context.Boards.Add(newBoard);
            await _context.SaveChangesAsync();

            var ownerMember = new BoardMember
            {
                BoardId = newBoard.Id,
                UserId = user.Id,
                JoinedAt = DateTime.Now,
                Role = "Admin"
            };
            _context.BoardMembers.Add(ownerMember);
            await _context.SaveChangesAsync();

            return Json(new { success = true, boardId = newBoard.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create board error");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> CheckBoardPassword(string code)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length != 6)
            {
                return Json(new { requiresPassword = false, exists = false });
            }

            var board = await _context.Boards
                .FirstOrDefaultAsync(b => b.JoinCode == code);
            
            if (board == null)
            {
                return Json(new { requiresPassword = false, exists = false });
            }
            
            return Json(new 
            { 
                requiresPassword = board.IsPrivate && !string.IsNullOrEmpty(board.Password),
                exists = true 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking board password");
            return Json(new { requiresPassword = false, exists = false, error = true });
        }
    }

    [HttpGet]
    public IActionResult Join()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> JoinBoard(DashboardAccessVm model)
    {
        if (string.IsNullOrWhiteSpace(model.AccessCode) || model.AccessCode.Length != 6)
        {
            TempData["ErrorMessage"] = "Please enter a valid 6-character access code.";
            return RedirectToAction("Access");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var board = await _context.Boards
            .Include(b => b.BannedUsers)
            .FirstOrDefaultAsync(b => b.JoinCode == model.AccessCode.ToUpper());

        if (board == null)
        {
            TempData["ErrorMessage"] = "This board does not exist. Please check the access code.";
            return RedirectToAction("Access");
        }

        var isBanned = board.BannedUsers != null &&
                       board.BannedUsers.Any(b => b.UserId == user.Id);

        if (isBanned)
        {
            TempData["ErrorMessage"] = "You have been banned from this board and cannot join.";
            return RedirectToAction("Access");
        }

        if (board.OwnerId == user.Id)
        {
            TempData["SuccessMessage"] = "Welcome back to your board!";
            return RedirectToAction("Index", "Board", new { id = board.Id });
        }

        var isAlreadyMember = await _context.BoardMembers
            .AnyAsync(m => m.BoardId == board.Id && m.UserId == user.Id);

        if (isAlreadyMember)
        {
            TempData["SuccessMessage"] = "Welcome back!";
            return RedirectToAction("Index", "Board", new { id = board.Id });
        }

        if (board.IsPrivate && !string.IsNullOrEmpty(board.Password))
        {
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                TempData["ErrorMessage"] = "This board requires a password.";
                model.RequiresPassword = true;
                model.AccessCode = model.AccessCode; 
                return View("Access", model);
            }

            if (model.Password != board.Password)
            {
                TempData["ErrorMessage"] = "Incorrect password for this private board.";
                model.RequiresPassword = true;
                model.AccessCode = model.AccessCode; 
                return View("Access", model);
            }
        }

        var newMember = new BoardMember
        {
            BoardId = board.Id,
            UserId = user.Id,
            JoinedAt = DateTime.Now,
            Role = "Member"
        };

        _context.BoardMembers.Add(newMember);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Successfully joined {board.Name}!";
        return RedirectToAction("Index", "Board", new { id = board.Id });
    }

    private string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}