using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskUp.Data;
using TaskUp.ViewModels.Admin;

namespace TaskUp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Now.Date.AddDays(-i))
                .OrderBy(d => d)
                .ToList();

            var model = new AdminDashboardVM
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalBoards = await _context.Boards.CountAsync(),
                TotalTasks = await _context.BoardTasks.CountAsync(),
                TotalPersonalBoards = await _context.Boards
                    .CountAsync(b => b.BoardType == Utilities.Enums.BoardType.Personal),
                TotalTeamBoards = await _context.Boards
                    .CountAsync(b => b.BoardType == Utilities.Enums.BoardType.Team),
                
                RecentUsers = await _context.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                
                RecentBoards = await _context.Boards
                    .Include(b => b.Owner)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                
                Last7Days = last7Days.Select(d => d.ToString("MMM dd")).ToList(),
                
                UserGrowth = await GetUserGrowthAsync(last7Days)
            };

            return View(model);
        }

        private async Task<List<int>> GetUserGrowthAsync(List<DateTime> last7Days)
        {
            var growth = new List<int>();
            
            foreach (var day in last7Days)
            {
                var count = await _context.Users
                    .CountAsync(u => u.CreatedAt.Date == day.Date);
                growth.Add(count);
            }
            
            return growth;
        }
    }
}