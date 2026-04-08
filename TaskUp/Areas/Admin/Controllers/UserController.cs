using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskUp.Services;

namespace TaskUp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly AdminService _adminService;

        public UserController(AdminService adminService)
        {
            _adminService = adminService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _adminService.GetAllUsersWithRoles();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> MakeAdmin([FromBody] MakeAdminRequest request)
        {
            try
            {
                Console.WriteLine($"MakeAdmin called with userId: {request?.UserId}");
        
                if (request == null || string.IsNullOrEmpty(request.UserId))
                {
                    return Json(new { success = false, message = "UserId is required" });
                }
        
                var result = await _adminService.MakeAdminAsync(request.UserId);
                return Json(new { success = result, message = result ? "Success" : "Failed" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MakeAdmin: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveAdmin([FromBody] MakeAdminRequest request)
        {
            try
            {
                Console.WriteLine($"RemoveAdmin called with userId: {request?.UserId}");
        
                if (request == null || string.IsNullOrEmpty(request.UserId))
                {
                    return Json(new { success = false, message = "UserId is required" });
                }
        
                var result = await _adminService.RemoveAdminAsync(request.UserId);
                return Json(new { success = result, message = result ? "Success" : "Failed" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RemoveAdmin: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class MakeAdminRequest
        {
            public string UserId { get; set; }
        }
    }
}