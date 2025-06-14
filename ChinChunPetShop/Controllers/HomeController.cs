using ChinChunPetShop.Models;
using ChinChunPetShop.Models.Configs;
using ChinChunPetShop.Models.Entity;
using ChinChunPetShop.Models.Services;
using ChinChunPetShop.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ChinChunPetShop.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
		private readonly SanPhamService _spService;
		private readonly ThamSoService _tsService;
        private readonly SecurityService _security;

        public HomeController(ILogger<HomeController> logger, SessionConfig session, SanPhamService service, ThamSoService thamSoService, SecurityService security) : base(session)
        {
            _logger = logger;
            _spService = service;
            _tsService = thamSoService;
            _security = security;
        }

        public async Task<IActionResult> Index()
        {
            var sp = await _spService.SanPhamBestSelling();
            ViewBag.message = TempData["message"] as string;
            return View(sp);
        }

		[Route("sanpham")]
		public async Task<IActionResult> SanPham(int page = 0, string sortby = "tensp", string sort_type = "ASC", string tensp = "", string nhanhieu = "", string loaisp = "", string giamin = "", string giamax = "")
        {
			ViewBag.tensp = tensp;
			ViewBag.nhanhieu = nhanhieu;
			ViewBag.NhanHieus = await _spService.GetDB().NhanHieus.ToListAsync();
			ViewBag.loaisp = loaisp;
			ViewBag.LoaiSanPhams = await _spService.GetDB().LoaiSanPhams.OrderBy(m => m.MaLoaiSP).ToListAsync();
			ViewBag.page_index = page;
			ViewBag.sortby = sortby;
			ViewBag.sort_type = sort_type;
			int size_page = 12;
			var sanpham = _spService.GetDB().SanPhams.Include(t => t.MaNhanHieuNavigation).Include(t => t.MaLoaiSPNavigation).Include(m=>m.TonKhos).AsQueryable();
			
			if (giamin == "" || string.IsNullOrEmpty(giamin))
			{
				ViewBag.giamin = "";
				giamin = "0";
			}
			else
			{
				ViewBag.giamin = giamin;
			}
			if (giamax == "" || string.IsNullOrEmpty(giamax))
			{
				giamax = Int32.MaxValue.ToString();
				ViewBag.giamax = "";// Int32.MaxValue.ToString(); 
			}
			else
			{
				ViewBag.giamax = giamax;
			}
			if (!string.IsNullOrEmpty(tensp))
			{
				sanpham = sanpham.Where(t => t.TenSP.Contains(tensp));
			}
			if (!string.IsNullOrEmpty(nhanhieu))
			{
				sanpham = sanpham.Where(t => t.MaNhanHieu == nhanhieu);
			}

			if (!string.IsNullOrEmpty(loaisp))
			{
				sanpham = sanpham.Where(t => t.MaLoaiSP == loaisp);
			}

			if (decimal.TryParse(giamin, out decimal minPrice))
			{
				sanpham = sanpham.Where(t => t.GiaBanLe >= minPrice);
			}

			if (decimal.TryParse(giamax, out decimal maxPrice))
			{
				sanpham = sanpham.Where(t => t.GiaBanLe <= maxPrice);
			}
			List<SanPham> dssp = await sanpham.ToListAsync();
			if (sort_type == "ASC")
			{
				if (sortby == "gia")
				{
					dssp.OrderBy(sp => sp.GiaBanLe);
				}
				else dssp.OrderBy(sp => sp.TenSP);
			}
			else
			{
				if (sortby == "gia")
				{
					dssp.OrderByDescending(sp => sp.GiaBanLe);
				}
				else dssp.OrderByDescending(sp => sp.TenSP);
			}
			ViewBag.total_product = dssp.Count;
            ViewBag.total_page = (int)Math.Ceiling((double)dssp.Count / size_page);
            List<SanPham> dssp_sub = dssp.Skip(page* size_page).Take(size_page).ToList();
			ViewBag.message = TempData["message"] as string;
			return View(dssp_sub);
		}

        [Route("sanpham/{masp}")]
        public async Task<IActionResult> SanPhamChiTiet(string masp)
		{
			if(masp == null)
			{
				return NotFound();
			}
            var sp = await _spService.GetCTSanPham(masp);
			if (sp == null) {
				return NotFound();
			}
			if (String.IsNullOrEmpty(sp.HinhAnh))sp.HinhAnh = "petshop_sp.png";
            ViewBag.sp_best_similar = await _spService.SanPhamBestSimilar(masp);
			ViewBag.phi_giao_hang = await _tsService.getPhiGiaoHang(sp.KhoiLuong??1000);
			ViewBag.so_luong_mua_si_toi_thieu = await _tsService.GetDB().ThamSos.Where(m => m.MaTS == "TS000000").Select(m => m.SLMuaSiToiThieu).SingleOrDefaultAsync();
            ViewBag.message = TempData["message"] as string;
            return View(sp);
		}

        

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
