using ChinChunPetShop.Controllers;
using ChinChunPetShop.Models.Configs;
using ChinChunPetShop.Models.Entity;
using ChinChunPetShop.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ChinChunPetShop.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Route("admin")]
    public class HomeAdminController : BaseController
    {
        private readonly DonHangService _dhService;
        private readonly KhachHangService _khService;
        private readonly SanPhamService _spService;
        private readonly ThamSoService _tsService;
        public HomeAdminController(SessionConfig session, DonHangService dhService, KhachHangService khService, SanPhamService spService, ThamSoService tsService) : base(session)
        {
            _dhService = dhService;  
            _khService = khService;
            _spService = spService;
            _tsService = tsService;
        }

        [Authorize(Policy = "Admin")]
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var user = _session.getNhanVien();
            if (user == null) {
                return Redirect("~/nhanvien/dangnhap");
            }
            ViewBag.rkey = "tuần";
            ViewBag.DoanhThuTheoDH = await _dhService.getGetDTTLDH();
            ViewBag.DonHangGT = await _dhService.GetSLDHGT();
            ViewBag.DoanhThuGT = await _dhService.GetSLTNGT();
            ViewBag.BanHangGT = await _spService.GetSLBanGT();
            ViewBag.DSSanPham = await _spService.GetSLMua();
            ViewBag.DSDonHangTheoNH = await _spService.GetSLDHNH();
            ViewBag.KhachHangGT = await _khService.GetSLKHGT();
            ViewBag.DSKhachHang = await _khService.GetSLMKH();
            var tkdh = await _dhService.getThongKeSLDonHang();
            var tkdt = await _dhService.getThongKeLoiNhuan();
            ViewBag.ThongKeDonHang = tkdh.ToDictionary(kv => kv.Key, kv => kv.Value);
            var tkdt_temp = tkdt.ToDictionary(kv => kv.Key, kv => kv.Value);
            ViewBag.tg = _khService.getTG(tkdt_temp.Values.ToList());
            var tg = _khService.getTG(tkdt_temp.Values.ToList());
            ViewBag.tg = tg.Key;
            Dictionary<string, decimal> tkdt_ans = tkdt.ToDictionary(kv => kv.Key, kv => Math.Round((decimal)kv.Value / tg.Value, 2));
            ViewBag.pading =(int) Math.Ceiling(tkdt_ans.Values.ToList().Max() * (decimal)1.1);
            ViewBag.ThongKeDoanhThu = tkdt_ans;
            ViewBag.user = user;
            return View();
        }
		[Authorize(Policy = "Admin")]
		[HttpPost("")]
		public async Task<IActionResult> Index(string field = "cd", string key = "week")
		{
			var user = _session.getNhanVien();
			if (user == null)
			{
				return Redirect("~/nhanvien/dangnhap");
			}
            ViewBag.key = key;
            ViewBag.field = field;
            ViewBag.rkey = "tuần";
            if (key == "month")
            {
                ViewBag.rkey = "tháng";
            }
            else if(key == "quarter")
            {
                ViewBag.rkey = "quý";
            }
            else if(key == "year")
            {
                ViewBag.rkey = "năm";
            }
            ViewBag.DoanhThuTheoDH = await _dhService.getGetDTTLDH(field,key);
			ViewBag.DonHangGT = await _dhService.GetSLDHGT(field, key);
			ViewBag.DoanhThuGT = await _dhService.GetSLTNGT(field, key);
			ViewBag.BanHangGT = await _spService.GetSLBanGT(field, key);
			ViewBag.DSSanPham = await _spService.GetSLMua(field, key);
			ViewBag.DSDonHangTheoNH = await _spService.GetSLDHNH(field, key);
			ViewBag.KhachHangGT = await _khService.GetSLKHGT(field, key);
			ViewBag.DSKhachHang = await _khService.GetSLMKH(field, key);
            var tkdh = await _dhService.getThongKeSLDonHang(key);
            var tkdt = await _dhService.getThongKeLoiNhuan(key);
            ViewBag.ThongKeDonHang = tkdh.ToDictionary(kv => kv.Key, kv => kv.Value);
            var tkdt_temp = tkdt.ToDictionary(kv => kv.Key, kv => kv.Value);
            var tg = _khService.getTG(tkdt_temp.Values.ToList());
            ViewBag.tg = tg.Key;
            Dictionary<string, decimal> tkdt_ans = tkdt.ToDictionary(kv => kv.Key, kv => Math.Round((decimal)kv.Value / tg.Value,2));
            ViewBag.pading = (int)Math.Ceiling(tkdt_ans.Values.ToList().Max() * (decimal)1.1);
            ViewBag.ThongKeDoanhThu = tkdt_ans;
            ViewBag.user = user;
			return View();
		}
        [Authorize(Policy = "Admin")]
        [HttpGet("thamso")]
        public async Task<IActionResult> ThamSo()
        {
            var user = _session.getNhanVien();
            if (user == null)
            {
                return Redirect("~/nhanvien/dangnhap");
            }
            var dsts = await _tsService.GetDB().ThamSos.Where(m => m.MaTS == "TS000000").SingleOrDefaultAsync();
            ViewBag.message = TempData["message"] as string;
            return View(dsts);
        }
		[Authorize(Policy = "Admin")]
		[HttpGet("thamso/dieuchinh")]
		public async Task<IActionResult> DieuChinhThamSo()
		{
			var user = _session.getNhanVien();
			if (user == null)
			{
				return Redirect("~/nhanvien/dangnhap");
			}
			var dsts = await _tsService.GetDB().ThamSos.Where(m => m.MaTS == "TS000000").SingleOrDefaultAsync();
            return View(dsts);
		}
		[Authorize(Policy = "Admin")]
        [HttpPost("thamso/dieuchinh")]
        public async Task<IActionResult> DieuChinhThamSo(ThamSo update)
        {
            var user = _session.getNhanVien();
            if (user == null)
            {
                return Redirect("~/nhanvien/dangnhap");
            }
            var dsts = await _tsService.GetDB().ThamSos.Where(m => m.MaTS == "TS000000").SingleOrDefaultAsync();
            dsts.NgayCapNhat = DateTime.Now;
            dsts.TonKhoToiDa = update.TonKhoToiDa;
            dsts.TonKhoToiThieu = update.TonKhoToiThieu;
            dsts.TonKhoCanhBao = update.TonKhoCanhBao;
            dsts.GiaTriGhiDiem = update.GiaTriGhiDiem;
            dsts.ThoiGianTraHang = update.ThoiGianTraHang;
            dsts.SLMuaSiToiThieu = update.SLMuaSiToiThieu;
            dsts.ThoiGianTuDongHuyDon = update.ThoiGianTuDongHuyDon;
            await _tsService.GetDB().SaveChangesAsync();
            TempData["message"] = "Đã lưu lại thông tin tham số";
            return Redirect("~/admin/thamso");
        }
    }
}
