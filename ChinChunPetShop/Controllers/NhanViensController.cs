using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ChinChunPetShop.Models.Entity;
using ChinChunPetShop.Models.Configs;
using ChinChunPetShop.Models.Sevices;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ChinChunPetShop.Controllers
{

    public class NhanViensController : BaseController
    {
        private readonly NhanVienService _nvService;

        public NhanViensController(SessionConfig session, NhanVienService nvService) : base(session)
        {
            _nvService = nvService;
        }

        [Route("nhanvien/dangnhap")]
        public IActionResult Login()
        {
            ViewBag.error = "";
            ViewBag.message = TempData["message"] as string;
            return View();
        }
        [Route("nhanvien/dangnhap")]
        [HttpPost]
        public async Task<IActionResult> Login(NhanVien model, string next = "")
        {
            var taikhoan = await _nvService.TimKiem(model.Email, model.MatKhau);
            ViewBag.Email = model.Email;
            if (taikhoan != null)
            {
                var vt = await _nvService.GetDB().PhanQuyens.Where(m => m.MaNV == taikhoan.MaNV).Select(m => m.MaVT).ToListAsync();
                if (vt == null || vt.Count() == 0)
                {
                    ViewBag.TB = "Tài khoản của bạn chưa được cấp quyền truy cập vào hệ thống";
                    return View(model);
                }
                _session.setNhanVien(taikhoan, vt[0]);
                              
                TempData["message"] = "Đăng nhập thành công!";
                if (vt.Count() >= 2)
                {
                    return Redirect("~/nhanvien/chucnang");
                }

                if (next != null && next != "")
                {
                    return Redirect(next);
                }
                if (vt[0] == "VT000001")
                {
                    return Redirect("~/admin");
                }
                if (vt[0] == "VT000002")
                {
                    return Redirect("~/nvduyetdon");
                }
                if (vt[0] == "VT000003")
                {
                    return Redirect("~/nvbanhang");
                }
                _session.setNhanVien(null, null);
                ViewBag.TB = "Tài khoản của bạn chưa được cấp quyền truy cập vào hệ thống";
                return View(model);
            }

            ViewBag.TB = "*Địa chỉ email hoặc mật khẩu không đúng";
            return View(model);
        }

        [Route("admin/nhanvien")]
        [Authorize(Policy = "Admin")]
        // GET: NhanViens
        public async Task<IActionResult> Index(string manv="", string hoten="", string diachi="", string sdt="", string email="", string giamin="", string giamax = "")
        {
            var user = _session.getNhanVien();
            if (user == null)
            {
                return Redirect("~/nhanvien/dangnhap?next=~/admin/nhanvien");
            }
            ViewBag.manv = manv;
            ViewBag.hoten = hoten;
            ViewBag.VaiTros = _nvService.GetDB().VaiTros.ToList();
            IQueryable<NhanVien> nv = _nvService.GetDB().NhanViens.AsNoTracking(); // db là context của Entity Framework
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
            if (!string.IsNullOrEmpty(manv))
            {
                nv = nv.Where(m => m.MaNV.Contains(manv));
            }
            if (!string.IsNullOrEmpty(hoten))
            {
                nv = nv.Where(m => (m.HoNV + " " + m.TenNV).Contains(hoten));
            }
            if (!string.IsNullOrEmpty(diachi))
            {
                nv = nv.Where(m => m.DiaChi.Contains(diachi));
            }
            if (!string.IsNullOrEmpty(sdt))
            {
                nv = nv.Where(m => m.SDT == sdt);
            }
            if (!string.IsNullOrEmpty(email))
            {
                nv = nv.Where(m => m.Email == email);
            }
            if (decimal.TryParse(giamin, out decimal minPrice))
            {
                nv = nv.Where(t => t.Luong >= minPrice);
            }

            if (decimal.TryParse(giamax, out decimal maxPrice))
            {
                nv = nv.Where(t => t.Luong <= maxPrice);
            }

            List<NhanVien> dsnv = await nv.ToListAsync();

            // 4. Map từng NhanVien sang CTNhanVien (có thể dùng Task.WhenAll)
            var tasks = dsnv.Select(async nv => await _nvService.GetCTKH(nv))
                                .Where(t => (t) != null);
            var ctList =  (await Task.WhenAll(tasks))
                            .Where(ct => ct is not null)
                            .Select(ct => ct!) 
                            .ToList();
            ViewBag.message = TempData["message"] as string;
            return View(ctList);
        }

        // GET: NhanViens/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _nvService.GetDB().NhanViens
                .FirstOrDefaultAsync(m => m.MaNV == id);
            if (nhanVien == null)
            {
                return NotFound();
            }

            return View(nhanVien);
        }

        // GET: NhanViens/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: NhanViens/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaNV,HoNV,TenNV,Email,SDT,NgaySinh,GioiTinh,DiaChi,Luong,MatKhau,NgayVaoLam,XacThucEmail,MaXacThuc,ThoiHanMXT,HinhAnh")] NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                _nvService.GetDB().Add(nhanVien);
                await _nvService.GetDB().SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(nhanVien);
        }

        // GET: NhanViens/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _nvService.GetDB().NhanViens.FindAsync(id);
            if (nhanVien == null)
            {
                return NotFound();
            }
            return View(nhanVien);
        }

        // POST: NhanViens/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaNV,HoNV,TenNV,Email,SDT,NgaySinh,GioiTinh,DiaChi,Luong,MatKhau,NgayVaoLam,XacThucEmail,MaXacThuc,ThoiHanMXT,HinhAnh")] NhanVien nhanVien)
        {
            if (id != nhanVien.MaNV)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _nvService.GetDB().Update(nhanVien);
                    await _nvService.GetDB().SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NhanVienExists(nhanVien.MaNV))
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
            return View(nhanVien);
        }

        // GET: NhanViens/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _nvService.GetDB().NhanViens
                .FirstOrDefaultAsync(m => m.MaNV == id);
            if (nhanVien == null)
            {
                return NotFound();
            }

            return View(nhanVien);
        }

        // POST: NhanViens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var nhanVien = await _nvService.GetDB().NhanViens.FindAsync(id);
            if (nhanVien != null)
            {
                _nvService.GetDB().NhanViens.Remove(nhanVien);
            }

            await _nvService.GetDB().SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NhanVienExists(string id)
        {
            return _nvService.GetDB().NhanViens.Any(e => e.MaNV == id);
        }
    }
}
