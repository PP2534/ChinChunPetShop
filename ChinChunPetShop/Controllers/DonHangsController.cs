using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ChinChunPetShop.Models.Entity;
using ChinChunPetShop.Models.Configs;
using ChinChunPetShop.Models.Services;
using System.Text.Json;
using ChinChunPetShop.Models.ViewModel;

namespace ChinChunPetShop.Controllers
{
    public class DonHangsController : BaseController
    {
        private readonly ChinChunPetShopContext _context;
        private readonly KhachHangService _khService;
        private readonly GioHangService _ghService;
        private readonly DonHangService _dhService;
        private readonly ThamSoService _tsService;

        public DonHangsController(SessionConfig session, ChinChunPetShopContext context, KhachHangService khService, GioHangService ghService, DonHangService dhService, ThamSoService tsService) : base(session)
        {
            _context = context;
            _khService = khService;
            _ghService = ghService;
            _dhService = dhService;
            _tsService = tsService;
        }
        [Route("khachhang/dondadat")]
        public async Task<IActionResult> DonHangDaDat()
        {
            var user = _session.getKhachHang();
            if (user == null)
            {
                return Redirect("~/khachhang/dangnhap?next=~/khachhang/dondadat");
            }
            ViewBag.messger = TempData["message"] as string;
            List<DonHang> t = new List<DonHang>();
            t = await _context.DonHangs.Where(m => m.MaKH == user.MaKH).Include(m => m.CTDHs).Include(m => m.MaTTDHNavigation).Include(m => m.MaPTTTNavigation).Include(m=>m.CTDHs).ThenInclude(m=>m.MaSPNavigation).ToListAsync();
            var TTDHS = await _context.TrangThaiDHs.Select(m => m.MaTTDH).ToListAsync();
            ViewBag.XuLy = t.Where(m=>m.MaTTDH == TTDHS[0]).ToList();
            ViewBag.DangGiao = t.Where(m => m.MaTTDH == TTDHS[1]).ToList();
            ViewBag.DaNhan = t.Where(m => m.MaTTDH == TTDHS[2]).ToList();
            ViewBag.DaHuy = t.Where(m => m.MaTTDH == TTDHS[3]).ToList();
            ViewBag.DaTra = t.Where(m => m.MaTTDH == TTDHS[4]).ToList();
            return View();
        }


        [Route("khachhang/giohang/them")]
        [HttpPost]
        public async Task<IActionResult> ThemGioHang(GioHang model, string next)
        {
            var user = _session.getKhachHang();
            if (user == null)
            {
                return Redirect("~/khachhang/dangnhap?next=" + next);
            }
            model.MaKH = user.MaKH;
            if (await _ghService.ThemGioHang(model))
            {
                var ts = await _ghService.GetDB().SanPhams.Where(m=>m.MaSP == model.MaSP).Select(m=>m.TenSP).SingleOrDefaultAsync();
                TempData["message"] = "Thêm \""+ ts +"\" vào giỏ hàng thành công!";
                return Redirect(next);
            }
            TempData["message"] = "Thêm giỏ hàng thất bại. Lỗi: " + _ghService.message;
            return Redirect(next);
        }

        [Route("khachhang/giohang")]
        public async Task<IActionResult> GioHang()
        {
            var user = _session.getKhachHang();
            if (user == null)
            {
                return Redirect("~/khachhang/dangnhap?next=~/khachhang/giohang");
            }
            ViewBag.messger = TempData["message"] as string;
            ViewBag.actives = true;
            ViewBag.TongTienMua = 0;
            ViewBag.PhiGiaoHang = 0;
            ViewBag.GH = await _context.GioHangs.Where(m => m.MaKH == user.MaKH).CountAsync();
            var giohang = await _ghService.GetGioHang(user.MaKH);
            return View(giohang);
        }
        [Route("khachhang/giohang")]
        [HttpPost]
        public async Task<IActionResult> GioHang(Dictionary<string, string> Checked, Dictionary<string, int> SoLuong)
        {
            var user = _session.getKhachHang();
            if (user == null)
            {
                return Redirect("~/khachhang/dangnhap?next=~/khachhang/giohang");
            }
            if (await _ghService.Update(SoLuong))
            {
                TempData["message"] = _ghService.message;
            }
            ViewBag.TongTienMua = 0;
            
            var tongkhoiluong = 0;
            Dictionary<string, string> Check = new Dictionary<string, string>();
            foreach (var x in Checked.Values)
            {
                var t = await _context.GioHangs.Where(m => m.MaGH == x).Include(m=>m.MaSPNavigation).SingleOrDefaultAsync();
                if (t != null)
                {
                    Check[t.MaGH] = t.MaGH;
                    ViewBag.TongTienMua += t.SoLuong * t.DonGia;
                    tongkhoiluong += (t.MaSPNavigation.KhoiLuong??0)*t.SoLuong;
                }
            }
            ViewBag.Checked = Check;
            ViewBag.PhiGiaoHang = await _tsService.getPhiGiaoHang(tongkhoiluong);
            ViewBag.GH = await _context.GioHangs.Where(m => m.MaKH == user.MaKH ).CountAsync();
            var giohang = await _ghService.GetGioHang(user.MaKH);
            return View(giohang);
        }
        [Route("khachhang/giohang/xoa")]
        [HttpPost]
        public async Task<IActionResult> XoaGioHang(string MaGioHang)
        {
            var user = _session.getKhachHang();
            if (user == null)
            {
                return Redirect("~/khachhang/dangnhap?next=~/khachhang/giohang");
            }
            var gh = await _context.GioHangs.Where(m => m.MaGH == MaGioHang).SingleOrDefaultAsync();
            if (gh != null)
            {
                _context.GioHangs.Remove(gh);
                _context.SaveChanges();
            }
            else
            {
                TempData["message"] = "Lỗi giỏ hàng không tồn tại";
            }
            return Redirect("~/khachhang/giohang");
        }
        [Route("khachhang/donhang/trahang")]
        [HttpPost]
        public async Task<IActionResult> TraHang(DoiTraHang model)
        {
            var user = _session.getKhachHang();
            if (user == null)
            {
                return Redirect("~/khachhang/dangnhap?next=~/khachhang/dondadat");
            }
            if ( await _dhService.TaoYCTH(model))
            {
                TempData["message"] = "Đã gửi yêu cầu trả hàng thành công";
                return Redirect("~/khachhang/dondadat");
            }
            TempData["message"] = _dhService.message;
            return Redirect("~/khachhang/dondadat/" + model.MaDH);
        }

        [Route("khachhang/donhang/huydon")]
        [HttpPost]
        public async Task<IActionResult> HuyDon(string MaDonHang)
        {
            var user = _session.getKhachHang();
            if (user == null)
            {
                return Redirect("~/khachhang/dangnhap?next=~/khachhang/dondadat");
            }
            if (await _dhService.UpdateDonHang(MaDonHang, null, "TTDH0004"))
            {
                TempData["message"] = "Đã hủy đơn hàng thành công";
                return Redirect("~/khachhang/dondadat");
            }
            TempData["message"] = _dhService.message;
            return Redirect("~/khachhang/dondadat/" + MaDonHang);
        }

        // GET: DonHangs
        public async Task<IActionResult> Index()
        {
            var chinChunPetShopContext = _context.DonHangs.Include(d => d.MaKHNavigation).Include(d => d.MaNVNavigation).Include(d => d.MaPTTTNavigation).Include(d => d.MaTTDHNavigation);
            return View(await chinChunPetShopContext.ToListAsync());
        }

        // GET: DonHangs/Details/5
        [Route("khachhang/dondadat/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var user = _session.getKhachHang();
            if (user == null)
            {
                return Redirect("~/khachhang/dangnhap?next=~/khachhang/dondadat");
            }
            if (id == null)
            {
                return NotFound();
            }
            await _dhService.checkUpdate(id);
            var donHang = await _context.DonHangs
                .Include(d => d.MaKHNavigation)
                .Include(d => d.MaNVNavigation)
                .Include(d => d.MaPTTTNavigation)
                .Include(d => d.MaTTDHNavigation)
                .FirstOrDefaultAsync(m => m.MaDH == id);
            if (donHang == null)
            {
                return NotFound();
            }

            ViewBag.CheckHuyDon = await _dhService.CheckHuyDon(id);
            ViewBag.CheckTraHang = await _dhService.CheckTraHang(id);

            if (donHang.MaKH != user.MaKH)
            {
                return Redirect("~/khachhang/dondadat");
            }
            ViewBag.messger = TempData["message"] as string;
            ViewBag.Time = await _tsService.getThoiGianThanhToan(id);

            var CTDH = await _dhService.GetCTDonHang(id);
            return View(CTDH);
        }

        [Route("khachhang/donhang/taodonhang")]
        [HttpPost]
        public async Task<IActionResult> TaoDonHang(List<string> GHS = null, GioHang gh = null, string back = null)
        {
            var user = _session.getKhachHang();
            if (user == null)
            {
                if (back != null)
                    return Redirect("~/khachhang/dangnhap?next=" + back);
                else return Redirect("~/khachhang/dangnhap");
            }
            if ((GHS == null || GHS.Count == 0) && gh == null)
            {
                TempData["message"] = "Lỗi địa chỉ truy cập";
                return Redirect("~/");
            }

            if (GHS != null && GHS.Count > 0)
            {
               var ghs = await _ghService.getDSCTGH(GHS);
               HttpContext.Session.SetString("GioHang", JsonSerializer.Serialize(ghs));
            }
            else
            {
                List<CTGioHang> t = new List<CTGioHang>();
                gh.MaKH = user.MaKH;
                var ctgh = await _ghService.getCTGHTemp(gh);
                if (ctgh == null)
                {
                    if (back != null)
                        return Redirect("~/khachhang/dangnhap?next=" + back);
                    else return Redirect("~/khachhang/dangnhap");
                }
                else
                {
                    t.Add(ctgh);
                }
                HttpContext.Session.SetString("GioHang", JsonSerializer.Serialize(t));

            }
            return Redirect("~/khachhang/donhang/thanhtoan");
        }

        [Route("khachhang/donhang/thanhtoan")]
        // GET: DonHangs/Create
        public async Task<IActionResult> Create()
        {
            var user = _session.getKhachHang();
            if (user == null)
            {
                return Redirect("~/khachhang/dangnhap");
            }
            ViewBag.KhachHang = user;
            var data = HttpContext.Session.GetString("GioHang");
            if (data == null)
            {
                TempData["message"] = "Đã có lỗi bộ nhớ ghs";
                return Redirect("~/");
            }
            List<CTGioHang> GHS = JsonSerializer.Deserialize<List<CTGioHang>>(data);
            DonHang dh = new DonHang();
            dh.DiaChiGiaoHang = user.DiaChi;
            dh.SDTNguoiNhan = user.SDT;
            dh.TenNguoiNhan = user.HoTen;
            dh.MaDH = await _dhService.GetnewID();
            ViewBag.DonHang = dh;
            ViewBag.PhiGiaoHang = await _tsService.getPhiGiaoHang(GHS);
            ViewBag.TongTienMua = _ghService.getTongSTGH(GHS);
            return View(GHS);
        }

        // POST: DonHangs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Route("khachhang/donhang/thanhtoan")]
        [HttpPost]
        public async Task<IActionResult> Create(DonHang donHang)
        {
            var user = _session.getKhachHang();
            if (user == null)
            {
                return Redirect("~/khachhang/dangnhap");
            }
            ViewBag.KhachHang = user;
            var data = HttpContext.Session.GetString("GioHang");
            if (data == null)
            {
                TempData["message"] = "Đã có lỗi bộ nhớ ghs";
                return Redirect("~/");
            }
            List<CTGioHang> GHS = JsonSerializer.Deserialize<List<CTGioHang>>(data);
            ViewBag.DiaChis = donHang.DiaChiGiaoHang; 
            ViewBag.PhiGiaoHang = await _tsService.getPhiGiaoHang(GHS);
            ViewBag.TongTienMua = _ghService.getTongSTGH(GHS);
            ViewBag.DonHang = donHang;
            //--------------------------------------
            var CTDH = new List<CTDH>();
            
            foreach (var key in Request.Form.Keys)
            {
                if (key.StartsWith("CTDH["))
                {
                    // Xử lý thủ công từng giá trị
                    var index = int.Parse(key.Substring(5, key.IndexOf(']') - 5)); // Lấy index
                    var property = key.Substring(key.IndexOf('.') + 1); // Lấy tên thuộc tính

                    // Tạo đối tượng ChiTietDonHang tương ứng
                    while (CTDH.Count <= index) CTDH.Add(new CTDH());
                    var item = CTDH[index];

                    if (property == "MaSP") item.MaSP = Request.Form[key];
                    if (property == "DonGia") item.DonGia = decimal.Parse(Request.Form[key]);
                    if (property == "SoLuong") item.SoLuong = int.Parse(Request.Form[key]);
                }
            }
            if (ModelState.IsValid)
            {
                donHang.MaPTTT = "PTTT0003";
                donHang.MaKH = user.MaKH;
                if (await _dhService.TaoDonHang(donHang, CTDH, user.MaKH))
                {
                    TempData["message"] = "Đã đặt hàng thành công";
                    return Redirect("~/donhang/chitiet/" + _dhService.MDH);
                }
                ViewBag.TB = _dhService.message;
            }
            else
            {
                ViewBag.TB = "Đã có lỗi không hợp lệ.";
            }
            
            return View(GHS);
        }




        //// GET: DonHangs/Edit/5
        //public async Task<IActionResult> Edit(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var donHang = await _context.DonHangs.FindAsync(id);
        //    if (donHang == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["MaKH"] = new SelectList(_context.KhachHangs, "MaKH", "MaKH", donHang.MaKH);
        //    ViewData["MaNV"] = new SelectList(_context.NhanViens, "MaNV", "MaNV", donHang.MaNV);
        //    ViewData["MaPTTT"] = new SelectList(_context.PTTTs, "MaPTTT", "MaPTTT", donHang.MaPTTT);
        //    ViewData["MaTTDH"] = new SelectList(_context.TrangThaiDHs, "MaTTDH", "MaTTDH", donHang.MaTTDH);
        //    return View(donHang);
        //}

        // POST: DonHangs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(string id, [Bind("MaDH,MaNV,MaKH,MaTTDH,MaPTTT,TenNguoiNhan,DiaChiGiaoHang,SDTNguoiNhan,PhiGiaoHang,NgayMuaHang,NgayNhanHang,GhiChu,LoaiDH,ThanhToan")] DonHang donHang)
        //{
        //    if (id != donHang.MaDH)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(donHang);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!DonHangExists(donHang.MaDH))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["MaKH"] = new SelectList(_context.KhachHangs, "MaKH", "MaKH", donHang.MaKH);
        //    ViewData["MaNV"] = new SelectList(_context.NhanViens, "MaNV", "MaNV", donHang.MaNV);
        //    ViewData["MaPTTT"] = new SelectList(_context.PTTTs, "MaPTTT", "MaPTTT", donHang.MaPTTT);
        //    ViewData["MaTTDH"] = new SelectList(_context.TrangThaiDHs, "MaTTDH", "MaTTDH", donHang.MaTTDH);
        //    return View(donHang);
        ////}

        //// GET: DonHangs/Delete/5
        //public async Task<IActionResult> Delete(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var donHang = await _context.DonHangs
        //        .Include(d => d.MaKHNavigation)
        //        .Include(d => d.MaNVNavigation)
        //        .Include(d => d.MaPTTTNavigation)
        //        .Include(d => d.MaTTDHNavigation)
        //        .FirstOrDefaultAsync(m => m.MaDH == id);
        //    if (donHang == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(donHang);
        //}

        //// POST: DonHangs/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(string id)
        //{
        //    var donHang = await _context.DonHangs.FindAsync(id);
        //    if (donHang != null)
        //    {
        //        _context.DonHangs.Remove(donHang);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool DonHangExists(string id)
        {
            return _context.DonHangs.Any(e => e.MaDH == id);
        }
    }
}
