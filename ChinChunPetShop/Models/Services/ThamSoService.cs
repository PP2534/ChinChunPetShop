using ChinChunPetShop.Models.Configs;
using ChinChunPetShop.Models.Entity;
using ChinChunPetShop.Models.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace ChinChunPetShop.Models.Services
{
    public class ThamSoService : DBConfig
    {
        public ThamSoService(ChinChunPetShopContext context) : base(context)
        {
        }
        public async Task<bool> checkSPTKMax(string masp, int soluong)
        {
            try
            {
                int current = await GetDB().TonKhos.Where(m => m.MaSP == masp).CountAsync();
                int max = await GetDB().ThamSos.Where(m=>m.MaTS=="TS000000").Select(m=>m.TonKhoToiDa).SingleOrDefaultAsync();
                return current + soluong <= max;
            }
            catch
            {
                message = "Lỗi check max ton kho";
                return false;
            }
        }

        public async Task<decimal> getPhiGiaoHang(int khoiluong){
            if (khoiluong == 0) return 0;
            List<PhiVanChuyen> phiVanChuyens = await GetDB().PhiVanChuyens.OrderBy(m=>m.KhoiLuongMin).ToListAsync();
            foreach(PhiVanChuyen pvc in phiVanChuyens)
            {
                if(pvc.KhoiLuongMax == null)
                {
                    int p = (int)Math.Ceiling((decimal)(khoiluong-pvc.KhoiLuongMin) / 1000);
                    return pvc.ChiPhi + p * pvc.ChiPhiThem??0;
                }
                if(pvc.KhoiLuongMin>=khoiluong && pvc.KhoiLuongMax <= khoiluong)
                {
                    return pvc.ChiPhi;
                }
            }
            return phiVanChuyens.Last().ChiPhi;
        }

        public async Task<decimal> getPhiGiaoHang(List<GioHang>? ghs)
        {
            var tongkhoiluong = 0;
            if (ghs == null) return 0;
            foreach (var gh in ghs)
            {
                var t = await db.GioHangs.Where(m => m.MaGH == gh.MaGH).Include(m => m.MaSPNavigation).SingleOrDefaultAsync();
                if (t != null)
                {
                    tongkhoiluong += (t.MaSPNavigation.KhoiLuong ?? 0) * t.SoLuong;
                }
            }
            return await getPhiGiaoHang(tongkhoiluong);
        }

        public async Task<decimal> getPhiGiaoHang(List<CTGioHang>? ghs)
        {
            var tongkhoiluong = 0;
            if (ghs == null) return 0;
            foreach (var gh in ghs)
            {
                var t = await db.GioHangs.Where(m => m.MaGH == gh.MaGH).Include(m => m.MaSPNavigation).SingleOrDefaultAsync();
                if (t != null)
                {
                    tongkhoiluong += (t.MaSPNavigation.KhoiLuong ?? 0) * t.SoLuong;
                }
            }
            return await getPhiGiaoHang(tongkhoiluong);
        }

        public async Task<int> getThoiGianThanhToan(String id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return 0;
                }
                DateTime? dh = await db.DonHangs.Where(m => m.MaDH == id).Select(m=>m.NgayMuaHang).SingleOrDefaultAsync();
                int time = await db.ThamSos.Where(m => m.MaTS == "TS000000").Select(m => m.ThoiGianTuDongHuyDon).SingleOrDefaultAsync();
                if (dh != null)
                {
                    DateTime th = dh??DateTime.Now;
                    th = th.AddHours(time);
                    TimeSpan thoiGianConLai = th - DateTime.Now;

                    int secondsLeft = (int)Math.Max(thoiGianConLai.TotalSeconds, 0);
                    return secondsLeft;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

    }
}
