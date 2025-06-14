using ChinChunPetShop.Models.Entity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;
using static System.Net.WebRequestMethods;
namespace ChinChunPetShop.Models.Configs
{
    public class SessionConfig
    {
        private readonly IHttpContextAccessor _http;

        public SessionConfig(IHttpContextAccessor httpContextAccessor)
        {
            _http = httpContextAccessor;
        }
        public void setKhachHang(KhachHang? user)
        {
            if (user == null)
                _http.HttpContext?.Session.Remove("kh");
            else
            {
                _http.HttpContext?.Session.SetString("kh", JsonSerializer.Serialize(user));
            }
           
        }

        public KhachHang? getKhachHang()
        {
            var json = _http.HttpContext?.Session.GetString("kh");
            return json is null
                ? null
                : JsonSerializer.Deserialize<KhachHang>(json);
        }
        public async void setNhanVien(NhanVien? user, string? vaiTro)
        {
            if (user == null || vaiTro ==null)
                _http.HttpContext?.Session.Remove("nv");
            else
            {
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, user.MaNV),
                    new Claim("VaiTro", vaiTro)
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                if(_http.HttpContext is not null)
                {
                    await _http.HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));
                }

               
            }
            _http.HttpContext?.Session.SetString("nv", JsonSerializer.Serialize(user));
        }

        public NhanVien? getNhanVien()
        {
            var json = _http.HttpContext?.Session.GetString("nv");
            return json is null
                ? null
                : JsonSerializer.Deserialize<NhanVien>(json);
        }
    }
}
