using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using ChinChunPetShop.Models.Entity;
using ChinChunPetShop.Models.Sevices;

namespace ChinChunPetShop.Models.Configs
{
    public class RoleUserAttribute : IAuthorizationFilter
    {
        private readonly string _maVaiTro;
        private readonly string _next;
        private readonly NhanVienService _nvService;

        public RoleUserAttribute(NhanVienService nvService, string maVaiTro = "", string next = "")
        {
         
            _maVaiTro = maVaiTro;
            _next = next;
            _nvService = nvService;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var session = httpContext.Session;
            var user = session.GetString("MaNV");

            if (string.IsNullOrEmpty(user))
            {
                var redirectUrl = string.IsNullOrEmpty(_next) ? "/elogin" : $"/elogin?next={_next}";
                context.Result = new RedirectResult(redirectUrl);
                return;
            }

            if (!string.IsNullOrEmpty(_maVaiTro))
            {
                var check = _nvService.GetDB().PhanQuyens
                                  .Any(pq => pq.MaNV == user && pq.MaVT == _maVaiTro);

                if (!check)
                {
                    context.Result = new RedirectResult("/elogin/dinhtuyen");
                }
            }
        }
    }
}
