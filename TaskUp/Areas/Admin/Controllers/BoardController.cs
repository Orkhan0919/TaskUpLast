using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskUp.Data;
using TaskUp.Models;
using TaskUp.ViewModels.Admin;

namespace TaskUp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BoardController : Controller
    {
        private readonly AppDbContext _context;

        public BoardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var boards = await _context.Boards
                .Include(b => b.Owner)
                .Include(b => b.Members)
                .Include(b => b.Columns)
                .ThenInclude(c => c.Tasks)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var viewModel = boards.Select(b => new BoardAdminVM
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                BoardType = b.BoardType.ToString(),
                OwnerEmail = b.Owner?.Email ?? "Unknown",
                MemberCount = b.Members?.Count ?? 0,
                TaskCount = b.Columns?.Sum(c => c.Tasks?.Count ?? 0) ?? 0,
                CreatedAt = b.CreatedAt,
                JoinCode = b.JoinCode,
                IsPrivate = b.IsPrivate
            }).ToList();

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBoard([FromBody] DeleteBoardRequest request)
        {
            try
            {
                Console.WriteLine($"DeleteBoard called with boardId: {request?.BoardId}");

                if (request == null || request.BoardId <= 0)
                {
                    return Json(new { success = false, message = "Invalid board ID" });
                }

                var board = await _context.Boards.FindAsync(request.BoardId);
                if (board == null)
                {
                    return Json(new { success = false, message = $"Board not found with ID: {request.BoardId}" });
                }

                _context.Boards.Remove(board);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Board deleted successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class DeleteBoardRequest
        {
            public int BoardId { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeOwner(int boardId, string newOwnerId)
        {
            try
            {
                var board = await _context.Boards.FindAsync(boardId);
                var newOwner = await _context.Users.FindAsync(newOwnerId);

                if (board == null || newOwner == null)
                    return Json(new { success = false, message = "Board or user not found" });

                board.OwnerId = newOwnerId;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Owner changed successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}