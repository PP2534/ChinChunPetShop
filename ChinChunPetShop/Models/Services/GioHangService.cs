using ChinChunPetShop.Models.Configs;
using ChinChunPetShop.Models.Entity;
using ChinChunPetShop.Models.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace ChinChunPetShop.Models.Services
{
    public class GioHangService:DBConfig
    {
        SanPhamService _spService;
        public GioHangService(ChinChunPetShopContext context, SanPhamService spService) : base(context)
        {
            _spService = spService;
        }

        public int GetSLGH()
        {
            try
            {
                return db.GioHangs.Count();
            }
            catch
            {
                return -1;
            }
        }

        public async Task<bool> ThemGioHang(GioHang model)
        {

            if (model.MaSP == null)
            {
                message = "*Mã sản phẩm không được trống!";
                return false;
            }
            if (model.MaKH == null)
            {
                message = "*Mã người dùng không được trống!";
                return false;
            }
            try
            {
                if(model.DonGia == 0)
                {
                    model.DonGia = await db.SanPhams.Where(m => m.MaSP == model.MaSP).Select(m => m.GiaBanLe).SingleOrDefaultAsync();
                }
                var et = await db.GioHangs.Where(m => m.MaKH == model.MaKH && m.MaSP == model.MaSP).SingleOrDefaultAsync();
                if (et != null)
                {
                    et.SoLuong += model.SoLuong;
                    db.SaveChanges();
                    return true;
                }
                model.MaGH = GetID("GH", GetSLGH() + 1);
                List<string> mgh = db.GioHangs.Select(m => m.MaGH).ToList();
                int i = 1;
                while (mgh.Contains(model.MaGH))
                {
                    model.MaGH = GetID("GH", GetSLGH() + 1 - i);
                    i++;
                }
                db.GioHangs.Add(model);
                db.SaveChanges();
                return true;
            }
            catch
            {
                message = "*Lỗi hệ thống!";
                return false;
            }
        }


        public async Task<List<GioHang>> GetGioHang(string mauser)
        {
            try
            {
                return await db.GioHangs.Where(m => m.MaKH == mauser).Include(m=>m.MaKHNavigation).Include(m=>m.MaSPNavigation).ThenInclude(m=>m.TonKhos).Include(m=>m.MaSPNavigation).ThenInclude(m=>m.MaLoaiSPNavigation).ToListAsync();
            }
            catch
            {
                return new List<GioHang>();
            }
        }
        public async Task<int> GetSLGHKH(string makhachhang)
        {
            try
            {
                return await db.GioHangs.Where(m => m.MaKH == makhachhang).CountAsync();
            }
            catch
            {
                return -1;
            }
        }

        public async Task<bool> Update(Dictionary<string, int> data)
        {
            try
            {
                foreach (var x in data)
                {
                    var gh = await db.GioHangs.Where(m => m.MaGH == x.Key).SingleOrDefaultAsync();
                    gh.SoLuong = x.Value;
                    db.SaveChanges();
                }
                return true;
            }
            catch
            {
                message = "Đã có lỗi.";
                return false;
            }
        }

        //public async Task<CTGioHang?> getCTGH(GioHang gh)
        //{
        //    try
        //    {
        //        CTGioHang ctgh = new CTGioHang();
        //        ctgh.MaGH = gh.MaGH;
        //        ctgh.MaSP = gh.MaSP;
        //        ctgh.MaKH = gh.MaKH;
        //        ctgh.MaSPNavigation = gh.MaSPNavigation;
        //        ctgh.MaKHNavigation  = gh.MaKHNavigation;
        //        ctgh.TenLSP = await _spService.GetTenNhanHieu(ctgh.MaSPNavigation.MaNhanHieu);
        //        ctgh.TenLSP = await _spService.GetTenLoaiSP(ctgh.MaSPNavigation.MaLoaiSP);
        //        return ctgh;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}
        public async Task<CTGioHang?> getCTGH(string MaGH)
        {
            try
            {
                var gh = await db.GioHangs.Where(m => m.MaGH == MaGH).SingleOrDefaultAsync();
                CTGioHang ctgh = new CTGioHang();
                ctgh.MaGH = gh.MaGH;
                ctgh.MaSP = gh.MaSP;
                //ctgh.MaKH = gh.MaKH;
                ctgh.SoLuong = gh.SoLuong;
                ctgh.DonGia = gh.DonGia;
                ctgh.MaSPNavigation = await db.SanPhams.Where(m => m.MaSP == ctgh.MaSP).SingleOrDefaultAsync();
                ctgh.MaSPNavigation.GioHangs = null;
                //ctgh.MaKHNavigation = await db.KhachHangs.Where(m => m.MaKH == ctgh.MaKH).SingleOrDefaultAsync();
                ctgh.TenNhanHieu = await _spService.GetTenNhanHieu(ctgh.MaSPNavigation.MaNhanHieu);
                ctgh.TenLSP = await _spService.GetTenLoaiSP(ctgh.MaSPNavigation.MaLoaiSP);
                return ctgh;
            }
            catch
            {
                return null;
            }
        }
        public async Task<CTGioHang?> getCTGHTemp(GioHang gh)
        {
            try
            {
                CTGioHang ctgh = new CTGioHang();
                ctgh.MaSP = gh.MaSP;
                ctgh.MaKH = gh.MaKH;
                ctgh.SoLuong = gh.SoLuong;
                ctgh.DonGia = gh.DonGia;
                ctgh.MaSPNavigation = await db.SanPhams.Where(m => m.MaSP == ctgh.MaSP).SingleOrDefaultAsync();
                ctgh.MaSPNavigation.GioHangs = null;
                ctgh.MaKHNavigation = await db.KhachHangs.Where(m => m.MaKH == ctgh.MaKH).SingleOrDefaultAsync();
                ctgh.MaKHNavigation.GioHangs = null;
                ctgh.TenLSP = await _spService.GetTenNhanHieu(ctgh.MaSPNavigation.MaNhanHieu);
                ctgh.TenLSP = await _spService.GetTenLoaiSP(ctgh.MaSPNavigation.MaLoaiSP);
                return ctgh;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<CTGioHang>> getDSCTGH(List<GioHang> ghs)
        {
            try
            {
                List<CTGioHang> ctgh = new List<CTGioHang>();
                foreach (GioHang gh in ghs) { 
                    CTGioHang? ct = await getCTGH(gh.MaGH);
                    if (ct != null)
                    {
                        ctgh.Add(ct);
                    }
                    
                }
                return ctgh;
            }
            catch
            {
                return new List<CTGioHang>();
            }
        }
        public async Task<List<CTGioHang>> getDSCTGH(List<string> GHS)
        {
            try
            {
                //var ghs = await db.GioHangs.Where(m => GHS.Contains(m.MaGH)).ToListAsync();
                List<CTGioHang> ctgh = new List<CTGioHang>();
                foreach (string gh in GHS) { 
                    CTGioHang? ct = await getCTGH(gh);
                    if (ct != null)
                    {
                        ctgh.Add(ct);
                    }
                    
                }
                return ctgh;
            }
            catch
            {
                return new List<CTGioHang>();
            }
        }

        public decimal getTongSTGH(List<GioHang>? ghs)
        {
            try
            {
                if (ghs != null)
                {
                    decimal tonggiatri = 0;
                    foreach(var gh in ghs) { 
                        tonggiatri = gh.DonGia* (decimal)gh.SoLuong;
                    }
                    return tonggiatri;
                }
                return 0;
            }
            catch
            {
                message = "Đã có lỗi lấy tổng số tiền trong giỏ hàng";
                return 0;
            }
        }
        public decimal getTongSTGH(List<CTGioHang>? ghs)
        {
            try
            {
                if (ghs != null)
                {
                    decimal tonggiatri = 0;
                    foreach(var gh in ghs) { 
                        tonggiatri = gh.DonGia* (decimal)gh.SoLuong;
                    }
                    return tonggiatri;
                }
                return 0;
            }
            catch
            {
                message = "Đã có lỗi lấy tổng số tiền trong giỏ hàng";
                return 0;
            }
        }

    }
}
