using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ChinChunPetShop.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using ChinChunPetShop.Models.Configs;
using ChinChunPetShop.Models.Services;
using ChinChunPetShop.Models.ViewModel;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.Json;

namespace ChinChunPetShop.Controllers
{
    public class SoKhoesController : BaseController
    {
        private readonly ChinChunPetShopContext _context;
        private readonly SoKhoService _skService;
        private readonly SanPhamService _spService;

        public SoKhoesController(ChinChunPetShopContext context, SoKhoService service, SessionConfig session, SanPhamService spService):base(session)
        {
            _context = context;
            _skService = service;
            _spService = spService;
        }
        [Authorize(Policy = "Admin")]
        [Route("admin/sokho")]
        // GET: SoKhoes
        public async Task<IActionResult> Index(DateTime? ngaybatdau, DateTime? ngayketthuc, bool? loaigiaodich, string lydo="", string manv="")
        {
            var user = _session.getNhanVien();
            if (user == null)
            {
                return Redirect("~/nhanvien/dangnhap?next=~/admin/sokho");
            }
            ViewBag.ngaybatdau = ngaybatdau;
            ViewBag.ngayketthuc = ngayketthuc;
            ViewBag.loaigiaodich = loaigiaodich;
            ViewBag.lydo = lydo;
            ViewBag.LyDos = await _context.SoKhos.Select(m => m.LyDoNhapXuat).ToListAsync();
            ViewBag.manv = manv;
            var sk = _context.SoKhos.AsQueryable();
            
            if (!string.IsNullOrEmpty(lydo))
            {
                sk = sk.Where(m=>m.LyDoNhapXuat.Contains(lydo));
            }
            if (!string.IsNullOrEmpty(manv))
            {
                sk = sk.Where(m => m.MaNV.Contains(manv));
            }
            if (ngaybatdau.HasValue)
            {
                sk = sk.Where(m=>m.NgayGiaoDich >= ngaybatdau);
            }
            if (ngayketthuc.HasValue)
            {
                sk = sk.Where(m=>m.NgayGiaoDich <= ngayketthuc);
            }
            if (loaigiaodich.HasValue)
            {
                sk = sk.Where(m=>m.LoaiGiaoDich == loaigiaodich);
            }
            List<SoKho> dssk = await sk.ToListAsync();
            List<CTSoKho> dsctsk = new List<CTSoKho>();
            foreach(var sks in dssk)
            {
                var skv = await _skService.GetCTSoKho(sks.MaSK);
                if(skv!=null)dsctsk.Add(skv);
            }
            return View(dsctsk);
        }

        //// GET: SoKhoes/Details/5
        //public async Task<IActionResult> Details(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var soKho = await _context.SoKhos
        //        .FirstOrDefaultAsync(m => m.MaSK == id);
        //    if (soKho == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(soKho);
        //}

        // GET: SoKhoes/Create
        [Authorize(Policy = "Admin")]
        [Route("admin/sokho/taonhapxuat")]
        public async Task<IActionResult> Create()
        {
            var user = _session.getNhanVien();
            if (user == null)
            {
                return Redirect("~/nhanvien/dangnhap?next=~/admin/sokho");
            }
            ViewBag.SoKho = new SoKho();
            ViewBag.SP = await _spService.DanhSachCTSanPham();
            List<CTCTGiaoDich> ctgd = new List<CTCTGiaoDich>();
            CTCTGiaoDich c1 = new CTCTGiaoDich();
            c1.SanPham = await _spService.GetCTSanPham("SP000001");
            c1.SoLuong = 2;
            c1.DonGia = 100;
            CTCTGiaoDich c2 = new CTCTGiaoDich();
            c2.SanPham = await _spService.GetCTSanPham("SP000004");
            c2.SoLuong = 5;
            c2.DonGia = 19900;
            ctgd.Add(c1);
            ctgd.Add(c2);
            return View(ctgd);
        }



        [Authorize(Policy = "Admin")]
        [Route("admin/sokho/taonhapxuat")]
        [HttpPost]
        public async Task<IActionResult> Create(SoKho soKho)
        {
            var user = _session.getNhanVien();
            if (user == null)
            {
                return Redirect("~/nhanvien/dangnhap?next=~/admin/sokho");
            }
            var CTGD = new List<CTGiaoDich>();

            foreach (var key in Request.Form.Keys)
            {
                if (key.StartsWith("CTGD["))
                {
                    // Xử lý thủ công từng giá trị
                    var index = int.Parse(key.Substring(5, key.IndexOf(']') - 5)); // Lấy index
                    var property = key.Substring(key.IndexOf('.') + 1); // Lấy tên thuộc tính

                    // Tạo đối tượng ChiTietDonHang tương ứng
                    while (CTGD.Count <= index) CTGD.Add(new CTGiaoDich());
                    var item = CTGD[index];

                    if (property == "MaSP") item.MaSP = Request.Form[key];
                    if (property == "DonGia") item.DonGia = decimal.Parse(Request.Form[key]);
                    if (property == "SoLuong") item.SoLuong = int.Parse(Request.Form[key]);
                }
            }
            if (ModelState.IsValid)
            {
                _context.Add(soKho);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.SoKho = soKho;
            ViewBag.SP = await _spService.DanhSachCTSanPham();
            return View();
        }

        //// GET: SoKhoes/Edit/5
        //public async Task<IActionResult> Edit(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var soKho = await _context.SoKhos.FindAsync(id);
        //    if (soKho == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(soKho);
        //}

        // POST: SoKhoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaSK,LoaiGiaoDich,NgayGiaoDich,GhiChu")] SoKho soKho)
        {
            if (id != soKho.MaSK)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(soKho);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SoKhoExists(soKho.MaSK))
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
            return View(soKho);
        }

        // GET: SoKhoes/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var soKho = await _context.SoKhos
                .FirstOrDefaultAsync(m => m.MaSK == id);
            if (soKho == null)
            {
                return NotFound();
            }

            return View(soKho);
        }

        // POST: SoKhoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var soKho = await _context.SoKhos.FindAsync(id);
            if (soKho != null)
            {
                _context.SoKhos.Remove(soKho);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SoKhoExists(string id)
        {
            return _context.SoKhos.Any(e => e.MaSK == id);
        }
    }
}
