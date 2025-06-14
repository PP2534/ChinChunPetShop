using ChinChunPetShop.Models.Entity;
#nullable disable
namespace ChinChunPetShop.Models.ViewModel
{
    public class CTDonHang:DonHang
    {
        public decimal TongGiaTri { get; set; }
        public List<CTDH> CTDH { get; set; }
        public DoiTraHang DTH { get; set; }
        public bool HuyDon { get; set; }
        public bool TraHang { get; set; }
     
    }
}
