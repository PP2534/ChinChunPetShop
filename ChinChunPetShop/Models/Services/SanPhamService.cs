using System.Drawing.Imaging;
using System.Drawing;
using ChinChunPetShop.Models.Entity;
using ChinChunPetShop.Models.Configs;
using Microsoft.EntityFrameworkCore;
using ChinChunPetShop.Models.ViewModel;

namespace ChinChunPetShop.Models.Services
{
    public class SanPhamService : DBConfig
    {
        public SanPhamService(ChinChunPetShopContext context) : base(context)
        {
        }

        public async Task<List<SanPham>> DanhSachSanPham()
        {
            try
            {
                return await db.SanPhams.ToListAsync();
            }
            catch
            {
                return new List<SanPham>();
            }
        }

        public async Task<string?> GetnewID()
        {
            try
            {
                var sp = await db.SanPhams.ToListAsync();
                int sl = sp.Count;
                string id = GetID("SP", sl + 1);
                List<string> list = await db.SanPhams.Select(x => x.MaSP).ToListAsync();
                int i = 1;
                while (list.Contains(id))
                {
                    id = GetID("SP", sl + 1 - i);
                    i++;
                    if (i == sl + 1)
                    {
                        id = GetID("SP", sl + 2);
                        i = 3;
                        while (list.Contains(id))
                        {
                            id = GetID("SP", sl + i);
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

        public string? AddWhiteBorder(string inputPath, string outputPath, int canvasWidth = 880, int canvasHeight = 1330, int margin = 30)
        {
            try
            {
                // Load hình ảnh gốc
                using (Image originalImage = Image.FromFile(inputPath))
                {
                    // Tính toán kích thước hình ảnh bên trong khung
                    int maxWidth = canvasWidth - 2 * margin;
                    int maxHeight = canvasHeight - 2 * margin;

                    // Giữ nguyên tỷ lệ ảnh
                    float ratio = Math.Min((float)maxWidth / originalImage.Width, (float)maxHeight / originalImage.Height);
                    int scaledWidth = (int)(originalImage.Width * ratio);
                    int scaledHeight = (int)(originalImage.Height * ratio);

                    // Tạo khung trắng
                    using (Bitmap canvas = new Bitmap(canvasWidth, canvasHeight, PixelFormat.Format32bppArgb))
                    {
                        using (Graphics graphics = Graphics.FromImage(canvas))
                        {
                            // Tô nền trắng
                            graphics.Clear(Color.White);

                            // Căn giữa hình ảnh trong khung
                            int x = (canvasWidth - scaledWidth) / 2;
                            int y = (canvasHeight - scaledHeight) / 2;

                            // Vẽ hình ảnh lên canvas
                            graphics.DrawImage(originalImage, x, y, scaledWidth, scaledHeight);
                        }

                        // Sử dụng MemoryStream để lưu ảnh tạm thời trước khi ghi vào đĩa
                        using (MemoryStream ms = new MemoryStream())
                        {
                            canvas.Save(ms, ImageFormat.Png); // Lưu vào bộ nhớ
                            File.WriteAllBytes(outputPath, ms.ToArray()); // Ghi xuống file
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {

                return $"Lỗi: {ex.Message}";
            }
        }


        public async Task<List<SanPham>> SanPhamBestSelling()
        {
            try
            {
                var bestSellingSanPham =await (from ct in db.CTDHs
                                          join dh in db.DonHangs on ct.MaDH equals dh.MaDH
                                          group ct by ct.MaSP into SanPhamGroup
                                          select new
                                          {
                                              MaSP = SanPhamGroup.Key,
                                              TotalQuantity = SanPhamGroup.Sum(x => x.SoLuong)
                                          })
                                  .OrderByDescending(x => x.TotalQuantity) // Sắp xếp giảm dần theo tổng số lượng
                                  .Take(10) // Lấy 10 truyện
                                  .Join(db.SanPhams,
                                        top => top.MaSP,
                                        t => t.MaSP,
                                        (top, t) => t).Include(m=>m.MaNhanHieuNavigation).Include(m=>m.MaLoaiSPNavigation).Include(m=>m.TonKhos) // Liên kết để lấy thông tin truyện
                                  .ToListAsync();

                return bestSellingSanPham;
            }
            catch
            {
                return new List<SanPham>();
            }
        }

        public async Task<List<SanPham>> SanPhamBestSimilar(string masp)
        {
            try
            {
                var sp = await db.SanPhams.Where(m=>m.MaSP == masp).SingleOrDefaultAsync();
                if(sp != null)
                {
                    var dssp = await db.SanPhams.Where(m => m.MaLoaiSP == sp.MaLoaiSP).Take(5).ToListAsync();
                    dssp.AddRange(await db.SanPhams.Where(m => m.MaNhanHieu == sp.MaNhanHieu).Take(3).ToListAsync());
                    return dssp;
                }
                else
                {
                    return await SanPhamBestSelling();
                }
            }
            catch
            {
                return new List<SanPham>();
            }
        }

        public async Task<string?> GetTenSanPham(string MaSP)
        {
            try
            {
                return await db.SanPhams.Where(m => m.MaSP == MaSP).Select(m => m.TenSP).SingleOrDefaultAsync();
            }
            catch
            {
                return MaSP;
            }
        }
       
        public async Task <string?> GetTenNhanHieu(string maNhanHieu)
        {
            try
            {
                return await db.NhanHieus.Where(m => m.MaNhanHieu == maNhanHieu).Select(m => m.TenNhanHieu).SingleOrDefaultAsync();
            }
            catch
            {
                return maNhanHieu;
            }
        }

        public async Task<string?> GetTenLoaiSP(string maLoaiSP)
        {
            try
            {
                return await db.LoaiSanPhams.Where(m => m.MaLoaiSP==maLoaiSP).Select(m => m.TenLoaiSP).SingleOrDefaultAsync();
            }
            catch
            {
                return maLoaiSP;
            }
        }

        public async Task<decimal> GetGiaMua(string MaSP)
        {
            try
            {
                var t = await db.SanPhams.Where(m => m.MaSP == MaSP).SingleOrDefaultAsync();
                return t?.GiaBanLe??0;
            }
            catch
            {
                return 0;
            }
        }
        
        public async Task<List<int>> GetSL(string MaSP)
        {
            try
            {
                var t = await db.TonKhos.Where(m => m.MaSP == MaSP).SingleOrDefaultAsync();
                return [t?.SLTrongKho??0,t?.SLCoTheBan??0,t?.SLDaBan??0];
            }
            catch
            {
                return [0,0,0];
            }
        }
       
        public async Task <CTSanPham?> GetCTSanPham(string MaSP)
        {
            try
            {
                var sanpham = await db.SanPhams.Where(m => m.MaSP == MaSP).SingleOrDefaultAsync();
                var ans = new CTSanPham();
                ans.MaSP = sanpham?.MaSP;
                ans.TenSP = sanpham?.TenSP;
                ans.MaSKU = sanpham?.MaSKU;
                ans.MaNhanHieu = sanpham?.MaNhanHieu;
                ans.TenNhanHieu = await GetTenNhanHieu(ans.MaNhanHieu??"Chưa có nhãn hiệu");
                ans.MaLoaiSP = sanpham?.MaLoaiSP;
                ans.TenLoaiSP = await GetTenLoaiSP(ans.MaLoaiSP??"Không phân loại");
                ans.GiaBanLe = sanpham!.GiaBanLe;
                ans.GiaBanSi = sanpham!.GiaBanSi;
                ans.GiaNhap = sanpham!.GiaNhap;
                var t = await GetSL(MaSP);
                ans.SLTonKho = t[0];
                ans.SLCoTheBan = t[1];
                ans.SLDaBan = t[2];
                ans.KhoiLuong = sanpham?.KhoiLuong??0;
                ans.MoTa = sanpham?.MoTa;
                ans.DonViTinh = sanpham?.DonViTinh;
                ans.MaVach = sanpham?.MaVach;
                ans.GioiThieu = sanpham?.GioiThieu;
                ans.HinhAnh = sanpham?.HinhAnh != null ? sanpham.HinhAnh : "petshop_sp.png";
                return ans;
            }
            catch
            {
                return null;
            }

        }
        public async Task<List<CTSanPham>> DanhSachCTSanPham()
        {
            try
            {
                var sps = await db.SanPhams.ToListAsync();
                List<CTSanPham> dssp = new List<CTSanPham>();
                foreach(var sp in sps)
                {
                    CTSanPham? ctsp = await GetCTSanPham(sp.MaSP);
                    if (ctsp != null) dssp.Add(ctsp);
                    
                }
                return dssp;
            }
            catch
            {
                return new List<CTSanPham>();
            }
        }
        public async Task<KeyValuePair<int, decimal>> GetSLBanGT(string field="cd", string key = "week")
        {
            try
            {
                var lstime = getStartEndTime(field, key);
                DateTime startCurrentTime = lstime[0];
                DateTime endCurrentTime = lstime[1];
                DateTime startPreviousTime = lstime[2];
                DateTime endPreviousTime = lstime[3];

                int currentTimeOrders = 0;
                int previousTimeOrders = 0;

                var dhs =  await db.DonHangs.Where(dh => dh.NgayMuaHang >= startCurrentTime && dh.NgayMuaHang <= endCurrentTime && dh.MaTTDH=="TTDH0003").Select(m => m.MaDH).ToListAsync();
                foreach (var x in dhs)
                {
                    currentTimeOrders += await db.CTDHs.Where(m => m.MaDH == x).Select(m => m.SoLuong).SumAsync();
                }

                dhs = await db.DonHangs.Where(kh => kh.NgayMuaHang >= startPreviousTime && kh.NgayMuaHang <= endPreviousTime && kh.MaTTDH == "TTDH0003").Select(m => m.MaDH).ToListAsync();
                foreach (var x in dhs)
                {
                    previousTimeOrders += await db.CTDHs.Where(m => m.MaDH == x).Select(m => m.SoLuong).SumAsync();
                }

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

        public async Task<List<CTSanPham>> GetSLMua(string field = "cd", string key = "week")
        {
            try
            {
                var lstime = getStartEndTime(field, key);
                DateTime startCurrentTime = lstime[0];
                List<CTSanPham> ans = new List<CTSanPham>();
                var salesPerProduct = await db.CTDHs
                    .Where(ct =>
                        ct.MaDHNavigation.NgayMuaHang >= startCurrentTime
                        && ct.MaDHNavigation.MaTTDH == "TTDH0003")
                    .GroupBy(ct => ct.MaSP)
                    .Select(g => new
                    {
                        MaSP = g.Key,
                        SoLuongDaBan = g.Sum(ct => ct.SoLuong)
                    })
                    .ToListAsync();
                //var allProductsInfo = await db.SanPhams
                //    .Select(async sp => new CTSanPham
                //    {
                //        MaSP = sp.MaSP,
                //        TenSP = sp.TenSP,
                //        TenNhanHieu = sp.MaNhanHieuNavigation.TenNhanHieu,
                //        SLDaBan = 0 
                //    })
                //    .ToListAsync();
                List<CTSanPham> allProductsInfo = new List<CTSanPham> ();
                List<SanPham> dssp = await db.SanPhams.ToListAsync();
                foreach (var sp in dssp) {
                    CTSanPham? cTSanPham = await GetCTSanPham(sp.MaSP);
                    if(cTSanPham != null) allProductsInfo.Add(cTSanPham);
                }
                var dictSales = salesPerProduct
                        .ToDictionary(item => item.MaSP, item => item.SoLuongDaBan);

                foreach (var prodVm in allProductsInfo)
                {
                    prodVm.SLDaBan = 0;
                    if (dictSales.TryGetValue(prodVm.MaSP, out var soldQty))
                    {
                        prodVm.SLDaBan = soldQty;
                    }
                    
                }
                ans = allProductsInfo.OrderByDescending(x => x.SLDaBan).ToList();
                return ans;
            }
            catch
            {
                return new List<CTSanPham>();
            }
        }
        public async Task<List<CTNhanHieu>> GetSLDHNH(string field = "cd", string key = "week")
        {
            try
            {
                var lstime = getStartEndTime(field, key);
                DateTime startCurrentTime = lstime[0];
                List<CTNhanHieu> ans = new List<CTNhanHieu>();

                var salesPerBrand = await db.CTDHs
                    .Where(ct =>
                        ct.MaDHNavigation.NgayMuaHang >= startCurrentTime
                        && ct.MaDHNavigation.MaTTDH == "TTDH0003")
                    .GroupBy(ct => ct.MaSPNavigation.MaNhanHieu)
                    .Select(g => new
                    {
                        MaNhanHieu = g.Key,
                        SLDH = g.Select(ct => ct.MaDH).Distinct().Count()
                    })
                    .ToListAsync();
                var allBrands = await db.NhanHieus
                    .Select(nh => new CTNhanHieu
                    {
                        MaNhanHieu = nh.MaNhanHieu,
                        TenNhanHieu = nh.TenNhanHieu,
                        SLDH = 0
                    })
                    .ToListAsync();
                var dict = salesPerBrand.ToDictionary(x => x.MaNhanHieu, x => x.SLDH);
                foreach (var brandVm in allBrands)
                {
                    if (dict.TryGetValue(brandVm.MaNhanHieu, out int soldCount))
                    {
                        brandVm.SLDH = soldCount;
                    }
                }


                ans = allBrands.OrderByDescending(x => x.SLDH).ToList();
                return ans;
            }
            catch
            {
                return new List<CTNhanHieu>();
            }

        }
    }
}
