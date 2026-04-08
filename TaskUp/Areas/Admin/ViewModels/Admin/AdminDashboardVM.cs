using TaskUp.Models;

namespace TaskUp.ViewModels.Admin
{
    public class AdminDashboardVM
    {
        public int TotalUsers { get; set; }
        public int TotalBoards { get; set; }
        public int TotalTasks { get; set; }
        public int TotalPersonalBoards { get; set; }
        public int TotalTeamBoards { get; set; }
        
        public List<AppUser> RecentUsers { get; set; }
        public List<Board> RecentBoards { get; set; }
        
        public List<int> UserGrowth { get; set; }
        public List<string> Last7Days { get; set; }
    }
}