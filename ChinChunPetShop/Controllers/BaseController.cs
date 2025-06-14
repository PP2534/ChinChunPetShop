using ChinChunPetShop.Models.Configs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ChinChunPetShop.Controllers
{
    public class BaseController : Controller
    {
        protected readonly SessionConfig _session;

        public BaseController(SessionConfig session)
        {
            _session = session;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            ViewBag.CurrentNhanVien = _session.getNhanVien();
            ViewBag.CurrentKhachHang = _session.getKhachHang();
        }
    }
}
