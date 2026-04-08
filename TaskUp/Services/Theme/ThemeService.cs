using Microsoft.AspNetCore.Http;

namespace TaskUp.Services
{
    public interface IThemeService
    {
        string GetTheme();
        void SetTheme(string theme);
    }

    public class ThemeService : IThemeService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ThemeService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetTheme()
        {
            return _httpContextAccessor.HttpContext.Request.Cookies["Theme"] ?? "dark";
        }

        public void SetTheme(string theme)
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Append("Theme", theme, new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(30),
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });
        }
    }
}