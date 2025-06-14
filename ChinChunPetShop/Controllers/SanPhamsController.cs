using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ChinChunPetShop.Models.Entity;
using ChinChunPetShop.Models.Configs;
using Microsoft.AspNetCore.Authorization;
using ChinChunPetShop.Models.Services;
using ChinChunPetShop.Models.ViewModel;

namespace ChinChunPetShop.Controllers
{
    public class SanPhamsController : BaseController
    {
        private readonly ChinChunPetShopContext _context;
        private readonly SanPhamService _spService;
        private readonly ThamSoService _tsService;
        private readonly IWebHostEnvironment _env;
        public SanPhamsController(SessionConfig session, ChinChunPetShopContext context, SanPhamService sanPhamService, ThamSoService tsService, IWebHostEnvironment env) : base(session)
        {
            _spService = sanPhamService;
            _context = context;
            _tsService = tsService;
            _env = env;
        }

        [Authorize(Policy = "Admin")]
        [Route("admin/sanpham")]
        // GET: SanPhams
        public async Task<IActionResult> Index(string masp = "", string tensp = "", string masku="", string nhanhieu = "", string loaisp = "", string giamin="", string giamax="")
        {
            var user = _session.getNhanVien();
            if (user == null)
            {
                return Redirect("~/nhanvien/dangnhap?next=~/admin/sanpham");
            }
            ViewBag.masp = masp;
            ViewBag.masku = masku;
            ViewBag.tensp = tensp;
            ViewBag.nhanhieu = nhanhieu;
            ViewBag.NhanHieus = await _context.NhanHieus.ToListAsync();
            ViewBag.loaisp = loaisp;
            ViewBag.LoaiSanPhams = await _context.LoaiSanPhams.OrderBy(m => m.MaLoaiSP).ToListAsync();

            var sanpham = _context.SanPhams.Include(t => t.MaNhanHieuNavigation).Include(t => t.MaLoaiSPNavigation).AsQueryable();
            //IQueryable<SanPham> sanpham = _context.SanPhams.AsNoTracking();
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
			if (!string.IsNullOrEmpty(masp))
			{
				sanpham = sanpham.Where(t => t.MaSP.Contains(masp));
			}
			if (!string.IsNullOrEmpty(tensp))
            {
                sanpham = sanpham.Where(t => t.TenSP.Contains(tensp));
            }
            if (!string.IsNullOrEmpty(masku))
            {
                sanpham = sanpham.Where(t => t.MaSKU.Contains(masku));
            }

            if (!string.IsNullOrEmpty(nhanhieu))
            {
                sanpham = sanpham.Where(t => t.MaNhanHieu == nhanhieu);
            }

            if (!string.IsNullOrEmpty(loaisp))
            {
                sanpham = sanpham.Where(t=>t.MaLoaiSP == loaisp);
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
            //var tasks = dssp.Select(async sp => await _spService.GetCTSanPham(sp.MaSP))
            //                    .Where(t => (t) != null);
            //var ctList = (await Task.WhenAll(tasks))
            //                .Where(ct => ct is not null)
            //                .Select(ct => ct!)
            //                .ToList();
            List<CTSanPham> ans = new List<CTSanPham>();
            foreach(var sp in dssp)
            {
                var t = await _spService.GetCTSanPham(sp.MaSP);
                if(t != null) ans.Add(t);
            }
            ViewBag.message = TempData["message"] as string;
            TempData["message"] = null;
            return View(ans);
        }

        // GET: SanPhams/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams
                .Include(s => s.MaLoaiSPNavigation)
                .Include(s => s.MaNhanHieuNavigation)
                .FirstOrDefaultAsync(m => m.MaSP == id);
            if (sanPham == null)
            {
                return NotFound();
            }

            return View(sanPham);
        }

        [Authorize(Policy = "Admin")]
        [Route("admin/sanpham/them")]
        // GET: SanPhams/Create
        public IActionResult Create()
        {
            var user = _session.getNhanVien();
            if (user == null)
            {
                return Redirect("~/nhanvien/dangnhap?next=~/admin/sanpham");
            }
            ViewData["MaLoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoaiSP", "TenLoaiSP");
            ViewData["MaNhanHieu"] = new SelectList(_context.NhanHieus, "MaNhanHieu", "TenNhanHieu");
            return View();
        }
        [Authorize(Policy = "Admin")]
        [Route("admin/sanpham/them")]
        // POST: SanPhams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaSP,TenSP,MaSKU,MaLoaiSP,MaNhanHieu,KhoiLuong,MaVach,DonViTinh,MoTa,GiaBanLe,GiaBanSi,GiaNhap,HinhAnh")] SanPham sanPham, string? next)
        {
            var user = _session.getNhanVien();
            if (user == null)
            {
                return Redirect("~/nhanvien/dangnhap?next=~/admin/sanpham");
            }
            sanPham.MaSP = await _spService.GetnewID();
            var imgSP = Request.Form.Files["HinhAnh"];
            if (imgSP != null && imgSP.Length != 0)
            {
                string ext = Path.GetExtension(imgSP.FileName);
                string pFNSP = sanPham.MaSP + ext;
                var folderPath = Path.Combine(_env.WebRootPath, "images", "sanpham");
                Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, pFNSP);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imgSP.CopyToAsync(stream);
                }
                sanPham.HinhAnh = pFNSP;
            }
            else
            {
                sanPham.HinhAnh = "petshop_sp.png";
            }
            if (ModelState.IsValid)
            {
                _context.Add(sanPham);
                TonKho tk = new TonKho();
                tk.MaKho = "K0000001";
                tk.MaSP  = sanPham.MaSP;
                tk.SLDaBan = 0;
                tk.SLCoTheBan = 0;
                tk.SLTrongKho = 0;
                _context.TonKhos.Add(tk);
                await _context.SaveChangesAsync();
                TempData["message"] = "Thêm sản phẩm thành công";
                if (!string.IsNullOrEmpty(next))
                {
                    Redirect(next!);
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaLoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoaiSP", "TenLoaiSP", sanPham.MaLoaiSP);
            ViewData["MaNhanHieu"] = new SelectList(_context.NhanHieus, "MaNhanHieu", "TenNhanHieu", sanPham.MaNhanHieu);
            return View(sanPham);
        }

        // GET: SanPhams/Edit/5
        [Route("admin/sanpham/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            var user = _session.getNhanVien();
            if (user == null)
            {
                return Redirect("~/nhanvien/dangnhap?next=~/admin/sanpham/" + id);
            }
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham == null)
            {
                return NotFound();
            }
            ViewData["MaLoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoaiSP", "MaLoaiSP", sanPham.MaLoaiSP);
            ViewData["MaNhanHieu"] = new SelectList(_context.NhanHieus, "MaNhanHieu", "MaNhanHieu", sanPham.MaNhanHieu);
            return View(sanPham);
        }

        // POST: SanPhams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Route("admin/sanpham/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaSP,TenSP,MaSKU,MaLoaiSP,MaNhanHieu,KhoiLuong,MaVach,DonViTinh,MoTa,GiaBanLe,GiaBanSi,GiaNhap,HinhAnh")] SanPham sanPham)
        {
            var user = _session.getNhanVien();
            if (user == null)
            {
                return Redirect("~/nhanvien/dangnhap?next=~/admin/sanpham/" + id);
            }
            if (id != sanPham.MaSP)
            {
                return NotFound();
            }
            var imgSP = Request.Form.Files["HinhAnh"];
            if (imgSP != null && imgSP.Length != 0)
            {
                string ext = Path.GetExtension(imgSP.FileName);
                string pFNSP = sanPham.MaSP + ext;
                var folderPath = Path.Combine(_env.WebRootPath, "images", "sanpham");
                Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, pFNSP);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imgSP.CopyToAsync(stream);
                }
                sanPham.HinhAnh = pFNSP;
            }
            else
            {
                var ts = await _context.SanPhams.Where(m => m.MaSP == sanPham.MaSP).SingleOrDefaultAsync();
                if (ts != null)
                {
                    sanPham.HinhAnh = ts.HinhAnh;
                    _context.Entry(ts).State = EntityState.Detached; // Loại bỏ thực thể trùng lặp
                }
                else sanPham.HinhAnh = "petshop_sp.png";
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sanPham);
                    await _context.SaveChangesAsync();
                    TempData["message"] = "Cập nhật thông tin sản phẩm thành công";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SanPhamExists(sanPham.MaSP))
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
            ViewData["MaLoaiSP"] = new SelectList(_context.LoaiSanPhams, "MaLoaiSP", "MaLoaiSP", sanPham.MaLoaiSP);
            ViewData["MaNhanHieu"] = new SelectList(_context.NhanHieus, "MaNhanHieu", "MaNhanHieu", sanPham.MaNhanHieu);
            return View(sanPham);
        }

        // GET: SanPhams/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams
                .Include(s => s.MaLoaiSPNavigation)
                .Include(s => s.MaNhanHieuNavigation)
                .FirstOrDefaultAsync(m => m.MaSP == id);
            if (sanPham == null)
            {
                return NotFound();
            }

            return View(sanPham);
        }

        // POST: SanPhams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham != null)
            {
                _context.SanPhams.Remove(sanPham);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SanPhamExists(string id)
        {
            return _context.SanPhams.Any(e => e.MaSP == id);
        }
    }
}
