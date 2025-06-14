using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ChinChunPetShop.Models.Entity;
using ChinChunPetShop.Models.Services;
using ChinChunPetShop.Models.Configs;
using Microsoft.AspNetCore.Authorization;
using ChinChunPetShop.Models.ViewModel;

namespace ChinChunPetShop.Controllers
{
    public class KhachHangsController : BaseController
    {
        private readonly ChinChunPetShopContext _context;
        private readonly KhachHangService _khService;
        private readonly SecurityService _securityService;

        public KhachHangsController(SessionConfig session, ChinChunPetShopContext context, KhachHangService service, SecurityService securityService) : base(session)
        {
            _khService = service;
            _context = context;
            _securityService = securityService;
        }
        [Route("khachhang/dangnhap")]
        public IActionResult Login()
        {
            ViewBag.error = "";
            ViewBag.message = TempData["message"] as string;
            return View();
        }
        [Route("khachhang/dangnhap")]
        [HttpPost]
        public async Task<IActionResult> Login(KhachHang model, string next = "")
        {
            var taikhoan = await _khService.TimKiem(model.Email, model.MatKhau);
            ViewBag.Email = model.Email;
            if (taikhoan != null)
            {
                if (taikhoan.TrangThai == false)
                {
                    ViewBag.TB = "*Tài khoản của bạn đã bị khoá";
                    return View(model);
                }
                _session.setKhachHang(taikhoan);
                TempData["message"] = "Đăng nhập thành công!";
                if (taikhoan.XacThucEmail == false)
                {
                    return Redirect("~/khachhang/yeucauxacthuc");
                }
                if (!string.IsNullOrEmpty(next))
                {
                    return Redirect(next);
                }
                return Redirect("~/");
            }

            ViewBag.TB = "*Địa chỉ email hoặc mật khẩu không đúng";
            return View(model);
        }
        [Route("khachhang/yeucauxacthuc")]
        public ActionResult SendCode()
        {
            var user = _session.getKhachHang();
            if (user == null)
            {
                return Redirect("~/khachhang/dangnhap?next=~/khachhang/yeucauxacthuc");
            }
            ViewBag.TB = TempData["message"];
            return View(user);
        }
        [Route("khachhang/guixacthuc")]
        [HttpPost]
        public async Task<IActionResult> SendVerificationCode()
        {
            var user = _session.getKhachHang();
            if (user == null)
            {
                return Redirect("~/khachhang/dangnhap?next=~/khachhang/yeucauxacthuc");
            }
            // Tạo mã xác nhận (6 chữ số)
            var verificationCode = new Random().Next(100000, 999999).ToString();
            var profile = await _khService.GetDB().KhachHangs.Where(m => m.MaKH == user.MaKH).SingleOrDefaultAsync();
            // Lưu mã vào cơ sở dữ liệu hoặc Session (ví dụ: Session["VerificationCode"])
            profile.MaXacThuc = verificationCode;

            // Gửi email
            var emailSender = _securityService;
            var body_content = "<!DOCTYPE html>\r\n<html>\r\n  <body style=\"font-family: Arial, sans-serif; color: #333;\">\r\n    <h2 style=\"color: #2E86C1; margin-bottom: 0.5em;\">Xác nhận đăng ký tài khoản</h2>\r\n\r\n    <p style=\"margin-top: 0;\">Chào bạn,</p>\r\n\r\n    <p>\r\n      Cảm ơn bạn đã đăng ký tài khoản tại\r\n      <strong>ChinChun PetShop</strong>. Vui lòng nhập mã xác thực bên dưới\r\n      để hoàn tất quá trình đăng ký:\r\n    </p>\r\n\r\n    <div\r\n      style=\"\r\n        display: inline-block;\r\n        padding: 12px 20px;\r\n        margin: 20px 0;\r\n        background-color: #f4f6f8;\r\n        border-radius: 4px;\r\n        font-size: 20px;\r\n        font-weight: bold;\r\n        letter-spacing: 4px;\r\n      \"\r\n    >\r\n      " + verificationCode + "\r\n    </div>\r\n\r\n    <p style=\"margin-top: 1em;\">\r\n      <em>\r\n        Lưu ý: Mã xác thực có hiệu lực trong 3 phút. Nếu bạn không yêu cầu\r\n        đăng ký, vui lòng bỏ qua email này.\r\n      </em>\r\n    </p>\r\n\r\n    <p>Trân trọng,</p>\r\n    <p>\r\n      Đội ngũ <strong>ChinChun PetShop</strong><br />\r\n      Email hỗ trợ: <a href=\"mailto:phanphuong.hnq2014@gmail.com\">phanphuong.hnq2014@gmail.com</a>\r\n    </p>\r\n  </body>\r\n</html>\r\n";
            await emailSender.SendEmailAsync(user.Email, "ChinChunPetShop - Email xác thực tài khoản", body_content);
            profile.ThoiHanMXT = DateTime.Now.AddMinutes(3);
            _khService.GetDB().SaveChanges();
            _session.setKhachHang(profile);
            TempData["message"] = "Đã gửi mã xác thực.";
            return Redirect("~/khachhang/xacthuc"); // Trả về trang hiển thị thông báo
        }
        [Route("khachhang/xacthuc")]
        public ActionResult VerifyEmail()
        {
            ViewBag.TB = TempData["message"];
            var user = _session.getKhachHang();
            if (user == null)
            {
                return Redirect("~/khachhang/dangnhap?next=~/khachhang/xacthuc");
            }
            return View(user);
        }
        [Route("khachhang/xacthuc")]
        [HttpPost]
        public ActionResult VerifyEmail(string inputcode)
        {
            var user = _session.getKhachHang();
            if (user == null)
            {
                return Redirect("~/khachhang/dangnhap?next=~/khachhang/xacthuc");
            }
            var savedCode = user.MaXacThuc;

            if (DateTime.Now > user.ThoiHanMXT)
            {
                ViewBag.error2 = null;
                ViewBag.error = "Mã xác thực đã hết hạn";
                return View(user);
            }
            if (savedCode == inputcode)
            {
                // Xác nhận thành công
                var kh = _khService.GetDB().KhachHangs.Where(m => m.MaKH == user.MaKH).SingleOrDefault();
                TempData["message"] = "Xác thực thành công";
                kh!.XacThucEmail = true;
                _khService.GetDB().Entry(user).State = EntityState.Detached;
                _khService.GetDB().SaveChanges();
                // Thực hiện cập nhật trạng thái xác nhận email cho người dùng
                return Redirect("~/khachhang");
            }
            ViewBag.error2 = "Mã xác thực không đúng";
            return View(user);
        }
        [Route("admin/khachhang")]
        [Authorize(Policy = "Admin")]
        // GET: KhachHangs
        public async Task<IActionResult> Index(bool? trangthai, string makh = "", string hoten = "", string diachi = "", string sdt = "", string email = "", string diemmin = "", string diemmax = "")
        {

            var user = _session.getNhanVien();
            if (user == null)
            {
                return Redirect("~/nhanvien/dangnhap?next=~/admin/khachhang");
            }
            ViewBag.trangthai = trangthai;
            ViewBag.makh = makh;
            ViewBag.hoten = hoten;
            ViewBag.diachi = diachi;
            ViewBag.sdt = sdt;
            ViewBag.email = email;
            IQueryable<KhachHang> kh = _khService.GetDB().KhachHangs.AsNoTracking();
            if (diemmin == "" || string.IsNullOrEmpty(diemmin))
            {
                ViewBag.diemmin = "";
                diemmin = "0";
            }
            else
            {
                ViewBag.diemmin = diemmin;
            }
            if (diemmax == "" || string.IsNullOrEmpty(diemmax))
            {
                diemmax = Int32.MaxValue.ToString();
                ViewBag.diemmax = "";// Int32.MaxValue.ToString(); 
            }
            else
            {
                ViewBag.diemmax = diemmax;
            }
            if (!string.IsNullOrEmpty(makh))
            {
                kh = kh.Where(m => m.MaKH.Contains(makh));
            }
            if (!string.IsNullOrEmpty(hoten))
            {
                kh = kh.Where(m => m.HoTen.Contains(hoten));
            }
            if (!string.IsNullOrEmpty(diachi))
            {
                kh = kh.Where(m => m.DiaChi.Contains(diachi));
            }
            if (!string.IsNullOrEmpty(sdt))
            {
                kh = kh.Where(m => m.SDT == sdt);
            }
            if (!string.IsNullOrEmpty(email))
            {
                kh = kh.Where(m => m.Email == email);
            }
            if (trangthai!=null)
            {
                kh = kh.Where(m => m.TrangThai == trangthai); 
            }
            if (decimal.TryParse(diemmin, out decimal minPrice))
            {
                kh = kh.Where(t => t.Diem >= minPrice);
            }

            if (decimal.TryParse(diemmax, out decimal maxPrice))
            {
                kh = kh.Where(t => t.Diem <= maxPrice);
            }

            List<KhachHang> dskh = await kh.ToListAsync();

            //// 4. Map từng NhanVien sang CTNhanVien (có thể dùng Task.WhenAll)
            //var tasks = dskh.Select(kh => _khService.GetCTKH(kh))
            //                    .Where(t => (t) != null);
            //var ctList = (await Task.WhenAll(tasks))
            //                .Where(ct => ct is not null)
            //                .Select(ct => ct!)
            //                .ToList();
            List<CTKhachHang> ans = new List<CTKhachHang>();
            foreach(var khg in dskh)
            {
                var t = _khService.GetCTKH(khg);
                if(t != null) ans.Add(t);
            }
            
            ViewBag.message = TempData["message"] as string;
            return View(ans);

        }

        // GET: KhachHangs/Details/5
        [Route("khachhang")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(m => m.MaKH == id);
            if (khachHang == null)
            {
                return NotFound();
            }

            return View(khachHang);
        }

        [Route("khachhang/dangky")]
        // GET: KhachHangs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: KhachHangs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("khachhang/dangky")]
        public async Task<IActionResult> Create([Bind("MaKH,HoTen,SDT,Email,NgaySinh,GioiTinh,DiaChi,Diem,NgayTaoTK,MatKhau,TrangThai,XacThucEmail,MaXacThuc,ThoiHanMXT,HinhAnh")] KhachHang khachHang, string rMatKhau)
        {
            //if (ModelState.IsValid)
            //{
            //    _context.Add(khachHang);
            //    await _context.SaveChangesAsync();
            //    return RedirectToAction(nameof(Index));
            //}
            //return View(khachHang);
            if (khachHang.MatKhau != null && rMatKhau != khachHang.MatKhau)
            {
                ViewBag.error = "*Mật khẩu xác nhận không khớp";
                return View(khachHang);
            }
            if (await _khService.FindEmail(khachHang.Email))
            {
                ViewBag.error = "*Email này đã tồn tại";
                return View(khachHang);
            }
            if (ModelState.IsValid)
            {
                khachHang.MatKhau = _securityService.HashPassword(khachHang.MatKhau);
                khachHang.XacThucEmail = false;
                
               
                khachHang.MaKH = _khService.GetID("KH", await _khService.GetSLKH() + 1);
                List<string> mkh = _khService.GetDB().KhachHangs.Select(m => m.MaKH).ToList();
                int i = 1;
                while (mkh.Contains(khachHang.MaKH))
                {
                    khachHang.MaKH = _securityService.GetID("GH", await _khService.GetSLKH() + 1 - i);
                    i++;
                }
                khachHang.Diem = 0;
                khachHang.TrangThai = true;
                khachHang.NgayTaoTK = DateTime.Now;
                _khService.GetDB().KhachHangs.Add(khachHang);
                _khService.GetDB().SaveChanges();
                TempData["message"] = "Đã tạo tài khoản thành công.";
                return Redirect("~/khachhang/yeucauxacthuc");
            }
            return View(khachHang);
        }

        [Route("khachhang/chinhsuathongtin")]
        // GET: KhachHangs/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang == null)
            {
                return NotFound();
            }
            return View(khachHang);
        }

        // POST: KhachHangs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Route("khachhang/chinhsuathongtin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaKH,HoTen,SDT,Email,NgaySinh,GioiTinh,DiaChi,Diem,NgayTaoTK,MatKhau,TrangThai,XacThucEmail,MaXacThuc,ThoiHanMXT,HinhAnh")] KhachHang khachHang)
        {
            if (id != khachHang.MaKH)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(khachHang);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KhachHangExists(khachHang.MaKH))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(khachHang);
        }

        [Route("khachhang/dangxuat")]
        public ActionResult Logout()
        {
            _session.setKhachHang(null);
            TempData["message"] = "Đăng xuất thành công!";
            return Redirect("~/");
        }

        // GET: KhachHangs/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(m => m.MaKH == id);
            if (khachHang == null)
            {
                return NotFound();
            }

            return View(khachHang);
        }

        //// POST: KhachHangs/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(string id)
        //{
        //    var khachHang = await _context.KhachHangs.FindAsync(id);
        //    if (khachHang != null)
        //    {
        //        _context.KhachHangs.Remove(khachHang);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}
        public async Task<IActionResult> DeleteConfirmed()
        {
            var kh = _session.getKhachHang();
            if (kh == null)
            {
                return Redirect("~/khachhang/dangnhap?next=~/khachhang");
            }
            try
            {
                KhachHang khachHang = _securityService.GetDB().KhachHangs.Find(kh!.MaKH)!;
                _securityService.GetDB().KhachHangs.Remove(khachHang);
                _securityService.GetDB().SaveChanges();
                TempData["message"] = "Xoá tài khoản thành công";
                return Redirect("~/");
            }
            catch
            {
                TempData["messenger"] = "Tài khoản chưa thể bị xoá, vui lòng liên hệ quản trị viên để được giúp đỡ";
                return Redirect("~/khachhang/baomat");
            }
        }


        private bool KhachHangExists(string id)
        {
            return _context.KhachHangs.Any(e => e.MaKH == id);
        }
    }
}
