using ChinChunPetShop.Models.Configs;
using ChinChunPetShop.Models.Entity;
using ChinChunPetShop.Models.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChinChunPetShop.Models.Services
{
    public class KhachHangService:DBConfig
    {
        private readonly IOptions<SMTPSettings> _SMTPOption;
        private readonly SecurityService _security;
        public KhachHangService(ChinChunPetShopContext context, SecurityService securityService, IOptions<SMTPSettings> smtpOptions) : base(context)
        {
            _security = securityService;
            _SMTPOption = smtpOptions;
        }
        public async Task<bool> FindEmail(string email)
        {
            try
            {
                var tk = await db.KhachHangs.Where(m => m.Email == email).SingleOrDefaultAsync();
                if (tk != null)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public async Task<KhachHang?> TimKiem(string email, string password)
        {
            var user = await db.KhachHangs.Where(m => m.Email == email).ToListAsync();
            if (user.Count() > 0)
            {
                if (user[0].XacThucEmail == null) return await TimKiem_log(email, password);
                if (_security.VerifyPassword(user[0].MatKhau, password))
                    return user[0];
            }
            return null;
        }
        public async Task<KhachHang?> TimKiem_log(string email, string password)
        {
            var check = new SecurityService(_SMTPOption, db);
            var user = await db.KhachHangs.Where(m => m.Email == email && m.MatKhau == password).ToListAsync();
            if (user.Count() > 0)
            {
                user[0].MatKhau = check.HashPassword(user[0].MatKhau);
                user[0].XacThucEmail = false;
                db.SaveChanges();
                return user[0];
            }
            else { return null; }
        }

        public async Task<int> GetSLKH()
        {
            try
            {
                var t = await db.KhachHangs.ToListAsync();
                return t.Count();
            }
            catch { return 0; }
        }
        public CTKhachHang? GetCTKH(KhachHang kh)
        {
            try
            {
                CTKhachHang ctkh = new CTKhachHang();
                ctkh.MaKH = kh.MaKH;
                ctkh.HoTen = kh.HoTen;
                ctkh.NgaySinh = kh.NgaySinh;
                ctkh.GioiTinh = kh.GioiTinh;
                ctkh.Email = kh.Email;
                ctkh.SDT = kh.SDT;
                ctkh.MatKhau = kh.MatKhau;
                ctkh.DiaChi = kh.DiaChi;
                ctkh.XacThucEmail = kh.XacThucEmail;
                ctkh.MaXacThuc = kh.MaXacThuc;
                ctkh.ThoiHanMXT = kh.ThoiHanMXT;
                ctkh.NgayTaoTK = kh.NgayTaoTK;
                ctkh.HinhAnh = kh.HinhAnh;
                ctkh.Diem = kh.Diem;
                ctkh.TrangThai = kh.TrangThai;
                ctkh.GioHangs = kh.GioHangs;
                ctkh.SLGioHang = kh.GioHangs.Count;
                return ctkh;
            }
            catch
            {
                return null;
            }

        }

        public async Task<List<CTKhachHang>?> GetCTKHS()
        {
            try
            {
                List<KhachHang> khs = await db.KhachHangs.ToListAsync();
                List<CTKhachHang> khct = new List<CTKhachHang>();
                foreach (KhachHang x in khs)
                {
                    var t = GetCTKH(x);
                    if(t!=null)khct.Add(t);
                }
                return khct;
            }
            catch
            {
                return null;
            }
        }

        public async Task<KeyValuePair<int, decimal>> GetSLKHGT(string field = "cd", string key = "week")
        {
            try
            {
                var lstime = getStartEndTime(field, key);
                DateTime startCurrentTime = lstime[0];
                DateTime endCurrentTime = lstime[1];
                DateTime startPreviousTime = lstime[2];
                DateTime endPreviousTime = lstime[3];

                int currentTimeOrders = await db.KhachHangs
                                          .Where(kh => kh.NgayTaoTK >= startCurrentTime && kh.NgayTaoTK <= endCurrentTime).CountAsync();

                int previousTimeOrders = await db.KhachHangs
                                           .Where(kh => kh.NgayTaoTK >= startPreviousTime && kh.NgayTaoTK <= endPreviousTime)
                                           .CountAsync();

                // Tính phần trăm tăng/giảm
                decimal percentageChange = 0;

                if (previousTimeOrders > 0)
                {
                    percentageChange = ((decimal)(currentTimeOrders - previousTimeOrders) / previousTimeOrders) * 100;
                }
                else if (currentTimeOrders > 0)
                {

                    percentageChange = 100;
                }
                else
                {
                    percentageChange = 0;
                }

                return new KeyValuePair<int, decimal>(currentTimeOrders, percentageChange);
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu xảy ra lỗi
                message = "lỗi: " + ex.Message;
                return new KeyValuePair<int, decimal>(0, 0);
            }
        }

        public async Task<List<CTKhachHang>> GetSLMKH(string field = "cd", string key= "week")
        {
            try
            {
                var lstime = getStartEndTime(field, key);
                DateTime startCurrentTime = lstime[0];

                List<CTKhachHang>? ans = await GetCTKHS();
                if (ans != null)
                {
                    foreach (var t in ans)
                    {
                        var sl = await db.DonHangs.Where(m => m.MaKH == t.MaKH && m.NgayMuaHang >= startCurrentTime && m.MaTTDH=="TTDH0003").CountAsync();
                        t.SLM = sl;
                    }
                    ans = ans.OrderByDescending(x => x.SLM).ToList();
                    return ans;
                }
                return new List<CTKhachHang> { };
            }
            catch
            {
                return new List<CTKhachHang>();
            }

        }
    }
}
