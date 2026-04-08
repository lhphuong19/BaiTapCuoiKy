using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace SV22T1020337.Shop
{
    /// <summary>
    /// Thông tin tài khoản người dùng được lưu trong phiên đăng nhập (cookie)
    /// </summary>
    public class WebUserData
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? Photo { get; set; }
        public List<string>? Roles { get; set; }

        private List<Claim> Claims
        {
            get
            {
                var claims = new List<Claim>()
                {
                    new Claim(nameof(UserId),      UserId      ?? ""),
                    new Claim(nameof(UserName),    UserName    ?? ""),
                    new Claim(nameof(DisplayName), DisplayName ?? ""),
                    new Claim(nameof(Email),       Email       ?? ""),
                    new Claim(nameof(Photo),       Photo       ?? "")
                };
                if (Roles != null)
                    foreach (var role in Roles)
                        claims.Add(new Claim(ClaimTypes.Role, role));
                return claims;
            }
        }

        public ClaimsPrincipal CreatePrincipal()
        {
            var identity  = new ClaimsIdentity(Claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(identity);
        }
    }

    /// <summary>
    /// Extension để đọc WebUserData từ ClaimsPrincipal
    /// </summary>
    public static class WebUserExtensions
    {
        public static WebUserData? GetUserData(this ClaimsPrincipal principal)
        {
            try
            {
                if (principal?.Identity == null || !principal.Identity.IsAuthenticated)
                    return null;

                var data = new WebUserData
                {
                    UserId      = principal.FindFirstValue(nameof(WebUserData.UserId)),
                    UserName    = principal.FindFirstValue(nameof(WebUserData.UserName)),
                    DisplayName = principal.FindFirstValue(nameof(WebUserData.DisplayName)),
                    Email       = principal.FindFirstValue(nameof(WebUserData.Email)),
                    Photo       = principal.FindFirstValue(nameof(WebUserData.Photo)),
                    Roles       = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
                };
                return data;
            }
            catch { return null; }
        }
    }
}
