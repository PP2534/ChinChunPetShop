using ChinChunPetShop.Models.Configs;
using ChinChunPetShop.Models.Entity;
using ChinChunPetShop.Models.Services;
using ChinChunPetShop.Models.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ChinChunPetShop.Models.Sevices
{
    public class NhanVienService : DBConfig
    {
        private readonly IOptions<SMTPSettings> _SMTPOption;
        private readonly SecurityService _security;
        public NhanVienService(ChinChunPetShopContext context, SecurityService securityService, IOptions<SMTPSettings> smtpOptions) : base(context)
        {
            _security = securityService;
            _SMTPOption = smtpOptions;
        }
        public async Task<NhanVien?> TimKiem(string email, string password)
        {
             var user = await db.NhanViens.Where(m => m.Email == email).ToListAsync();
            if (user.Count() > 0)
            {
                
                if (user[0].XacThucEmail == null) return await TimKiem_log(email, password);
                if (_security.VerifyPassword(user[0].MatKhau, password))
                    return user[0];
            }
            return null;
        }
        public async Task<NhanVien?> TimKiem_log(string email, string password)
        {
            var check = new SecurityService(_SMTPOption, db);
            var user = await db.NhanViens.Where(m => m.Email == email && m.MatKhau == password).ToListAsync();
            if (user.Count() > 0)
            {
                user[0].MatKhau = check.HashPassword(user[0].MatKhau);
                user[0].XacThucEmail = false;
                db.SaveChanges();
                return user[0];
            }
            else { return null; }
        }

        public async Task<int> GetSLNV()
        {
            try
            {
                var kq = await db.NhanViens.ToListAsync();
                return kq.Count;
            }
            catch { return 0; }
        }

        public async Task<List<string>?> GetVaiTro(string manhanvien)
        {
            try
            {
                var vt = await db.PhanQuyens.Where(m => m.MaNV == manhanvien).Select(m => m.MaVT).ToListAsync();
                return await db.VaiTros.Where(m => vt.Contains(m.MaVT)).Select(m => m.TenVT).ToListAsync();
            }
            catch { return null; }
        }

        public async Task<CTNhanVien?> GetCTKH(NhanVien nv)
        {
            try
            {
                CTNhanVien ctnv = new CTNhanVien();
                ctnv.MaNV = nv.MaNV;
                ctnv.HoNV = nv.HoNV;
                ctnv.TenNV = nv.TenNV;
                ctnv.NgaySinh = nv.NgaySinh;
                ctnv.GioiTinh = nv.GioiTinh;
                ctnv.Email = nv.Email;
                ctnv.SDT = nv.SDT;
                ctnv.MatKhau = nv.MatKhau;
                ctnv.DiaChi = nv.DiaChi;
                ctnv.XacThucEmail = nv.XacThucEmail;
                ctnv.MaXacThuc = nv.MaXacThuc;
                ctnv.ThoiHanMXT = nv.ThoiHanMXT;
                ctnv.NgayVaoLam = nv.NgayVaoLam;
                ctnv.HinhAnh = nv.HinhAnh;
                ctnv.Luong = nv.Luong;
                ctnv.VaiTroNhanVien = await GetVaiTro(ctnv.MaNV);
                return ctnv;
            }
            catch
            {
                return null;
            }

        }

        public async Task<List<CTNhanVien>?> GetCTNVS()
        {
            try
            {
                List<NhanVien> nvs = await db.NhanViens.ToListAsync();
                List<CTNhanVien> nvct = new List<CTNhanVien>();
                foreach (NhanVien x in nvs)
                {
                    var t = await GetCTKH(x);
                    if (t is not null)
                    {
                        nvct.Add(t);
                    }
                }
                return nvct;
            }
            catch
            {
                return null;
            }
        }
        public async Task<string?> GetnewID()
        {
            try
            {
                var dsnv = await db.NhanViens.ToListAsync();
                int sl = dsnv.Count;
                string id = GetID("NV", sl + 1);
                List<string> list = await db.NhanViens.Select(x => x.MaNV).ToListAsync();
                int i = 1;
                while (list.Contains(id))
                {
                    id = GetID("NV", sl + 1 - i);
                    i++;
                    if (i == sl + 1)
                    {
                        id = GetID("NV", sl + 2);
                        i = 3;
                        while (list.Contains(id))
                        {
                            id = GetID("NV", sl + i);
                            i++;
                        }
                        return id;
                    }
                }
                return id;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UpdatePhanQuyen(string manhanvien, List<string> vaitro)
        {
            if (string.IsNullOrEmpty(manhanvien))
            {
                message = "Mã nhân viên không được để trống.";
                return false;
            }
            try
            {
                var phanquyen = await db.PhanQuyens.Where(m => m.MaNV == manhanvien).ToListAsync();
                if (phanquyen != null) db.PhanQuyens.RemoveRange(phanquyen);
                foreach (var x in vaitro)
                {
                    PhanQuyen pq = new PhanQuyen();
                    pq.MaNV = manhanvien;
                    pq.MaVT = x;
                    await db.PhanQuyens.AddAsync(pq);
                }
                await db.SaveChangesAsync();
                return true;
            }
            catch
            {
                message = "Đã có lỗi xảy ra.";
                return false;
            }



        }
        public async Task<string?> GetLayout(string manhanvien)
        {
            try
            {
                var vt = await db.PhanQuyens.Where(m => m.MaNV == manhanvien).Select(m => m.MaVT).ToListAsync();
                if (vt[0] == "VT000001")
                {
                    return "~/Areas/Admin/Views/Shared/_LayoutAdmin.cshtml";
                }
                if (vt[0] == "VT000002")
                {
                    return "~/Areas/NVDuyetDon/Views/Shared/_LayoutNVDuyetDon.cshtml";
                }
                if (vt[0] == "VT000003")
                {
                    return "~/Areas/NVDuyetDon/Views/Shared/_LayoutNVDuyetDon.cshtml";
                }
                if (vt[0] == "VT000004")
                {
                    return "~/Areas/NVDuyetDon/Views/Shared/_LayoutNVDuyetDon.cshtml";
                }
                return "~/";
            }
            catch { return null; }
        }
    }
}
