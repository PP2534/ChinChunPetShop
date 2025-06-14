using ChinChunPetShop.Models.Configs;
using ChinChunPetShop.Models.Entity;
using ChinChunPetShop.Models.Sevices;
using ChinChunPetShop.Models.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace ChinChunPetShop.Models.Services
{
    public class SoKhoService : DBConfig
    {
        public SoKhoService(ChinChunPetShopContext context) : base(context)
        {
        }
        public async Task<CTSoKho?> GetCTSoKho(string MaSK)
        {
            try
            {
                var sk = await db.SoKhos.Where(m => m.MaSK == MaSK).SingleOrDefaultAsync();
                var ans = new CTSoKho();
                ans.MaSK = sk!.MaSK;
                ans.LoaiGiaoDich = sk!.LoaiGiaoDich;
                ans.LyDoNhapXuat = sk!.LyDoNhapXuat;
                ans.NgayGiaoDich = sk.NgayGiaoDich;
                ans.MaNV = sk!.MaNV;
                ans.CTGiaoDiches = sk!.CTGiaoDiches;
                ans.GhiChu = sk!.GhiChu;
                ans.TenNV = await GetDB().NhanViens.Where(m => m.MaNV == ans.MaNV).Select(m=>m.TenNV).SingleOrDefaultAsync();
                ans.HoNV = await GetDB().NhanViens.Where(m => m.MaNV == ans.MaNV).Select(m=>m.HoNV).SingleOrDefaultAsync();
                return ans;
            }
            catch
            {
                return null;
            }

        }
    }
}
