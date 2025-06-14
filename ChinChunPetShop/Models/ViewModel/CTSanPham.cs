#nullable disable
using ChinChunPetShop.Models.Entity;

namespace ChinChunPetShop.Models.ViewModel
{
    public class CTSanPham : SanPham
    {
        public string TenNhanHieu { set; get; }
        public string TenLoaiSP { set; get; }
        public int SLTonKho {  set; get; }
        public int SLCoTheBan {  set; get; }
        public int SLDaBan { set; get; }
    }
}
