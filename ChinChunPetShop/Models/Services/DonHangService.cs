using ChinChunPetShop.Models.Configs;
using ChinChunPetShop.Models.Entity;
using ChinChunPetShop.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic;

namespace ChinChunPetShop.Models.Services
{
    public class DonHangService : DBConfig
    {
        private readonly ThamSoService _tsService;
        public DonHangService(ChinChunPetShopContext context, ThamSoService tsService) : base(context)
        {
            _tsService = tsService;
        }
        public string MDH { set; get; }


        public async Task<string?> GetnewID()
        {
            try
            {
                int sl =  db.DonHangs.ToList().Count;
                string id = GetID("DH", sl + 1);
                List<string> list = await db.DonHangs.Select(x => x.MaDH).ToListAsync();
                int i = 1;
                while (list.Contains(id))
                {
                    id = GetID("DH", sl + 1 - i);
                    i++;
                    if (i == sl + 1)
                    {
                        id = GetID("DH", sl + 2);
                        i = 3;
                        while (list.Contains(id))
                        {
                            id = GetID("DH", sl + i);
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
        public async Task<string?> GetnewIDTraHang()
        {
            try
            {
                int sl = db.DoiTraHangs.ToList().Count;
                string id = GetID("YCTH", sl + 1);
                List<string> list = await db.DoiTraHangs.Select(x => x.MaDT).ToListAsync();
                int i = 1;
                while (list.Contains(id))
                {
                    id = GetID("YCTH", sl + 1 - i);
                    i++;
                    if (i == sl + 1)
                    {
                        id = GetID("YCTH", sl + 2);
                        i = 3;
                        while (list.Contains(id))
                        {
                            id = GetID("YCTH", sl + i);
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

        
        public async Task<bool> TaoDonHang(DonHang dh, List<CTDH> ctdh, string makhachhang)
        {
            try
            {
                if (ctdh == null || ctdh.Count == 0)
                {
                    message = "Số lượng truyện không được bind đúng cách";
                    return false;
                }
                if (dh.LoaiDH==false && dh.PhiGiaoHang == null)
                {
                    message = "Thiếu phí giao hàng";
                    return false;
                }
                if (string.IsNullOrEmpty(dh.MaPTTT))
                {
                    message = "Thiếu phương thức thanh toán";
                    return false;
                }
                if (dh.LoaiDH == false && string.IsNullOrEmpty(dh.TenNguoiNhan))
                {
                    message = "Thiếu tên người nhận";
                    return false;
                }
                if (dh.LoaiDH == false && string.IsNullOrEmpty(dh.SDTNguoiNhan))
                {
                    message = "Thiếu số điện thoại người nhận";
                    return false;
                }
                if(dh.LoaiDH == false && string.IsNullOrEmpty(dh.DiaChiGiaoHang))
                {
                    message = "Thiếu địa chỉ giao hàng";
                    return false;
                }
                //MDH = GetnewID();
                dh.MaDH = await GetnewID();
                if (dh.LoaiDH)
                {
                    if (string.IsNullOrEmpty(makhachhang)){
                        dh.MaKH = "KH000000";
                    }
                    else
                    {
                        dh.MaKH = makhachhang;
                    }

                    dh.PhiGiaoHang = 0;
                    dh.MaTTDH = "TTDH0003";
                }
                else {
                    dh.MaKH = makhachhang;
                    dh.MaTTDH = "TTDH0001";
                }
                dh.NgayMuaHang = DateTime.Now;
               
                dh.ThanhToan = false;
                db.DonHangs.Add(dh);
                if (dh.LoaiDH == true)
                {
                    foreach (var t in ctdh)
                    {
                        t.MaDH = dh.MaDH;
                        dh.CTDHs.Add(t);
                        var tk = await db.TonKhos.Where(m => m.MaSP == t.MaSP).SingleOrDefaultAsync();
                        if (tk == null)
                        {
                            message = $"Sản phẩm {t.MaSP} không được xác định";
                            return false;
                        }
                        if (tk.SLCoTheBan < t.SoLuong)
                        {
                            message = $"Sản phẩm {t.MaSP} không đủ số lượng";
                            return false;
                        }
                        tk.SLDaBan += t.SoLuong;
                        tk.SLTrongKho -= t.SoLuong;
                        tk.SLCoTheBan -= t.SoLuong;
                    }
                }
                else
                {
                    foreach (var t in ctdh)
                    {
                        t.MaDH = dh.MaDH;
                        dh.CTDHs.Add(t);
                        var gh = db.GioHangs.Where(m => m.MaKH == makhachhang && m.MaSP == t.MaSP).SingleOrDefault();
                        if (gh != null) db.GioHangs.Remove(gh);
                        var tk = await db.TonKhos.Where(m => m.MaSP == t.MaSP).SingleOrDefaultAsync();
                        if (tk == null)
                        {
                            message = $"Sản phẩm {t.MaSP} không được xác định";
                            return false;
                        }
                        if (tk.SLCoTheBan < t.SoLuong)
                        {
                            message = $"Sản phẩm {t.MaSP} không đủ số lượng";
                            return false;
                        }
                        tk.SLDaBan += t.SoLuong;
                        tk.SLCoTheBan -= t.SoLuong;
                    }
                }


                db.SaveChanges();
                return true;
            }
            catch
            {
                message = "Đã có lỗi đặt hàng.";
                return false;
            }

        }

        public async Task<DoiTraHang?> GetDTH(string madonhang)
        {
            try
            {
                return await db.DoiTraHangs.Where(m => m.MaDH == madonhang).SingleOrDefaultAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> CheckHuyDon(string madonhang)
        {
            try
            {
                var dh = await db.DonHangs.Where(m => m.MaDH == madonhang).SingleOrDefaultAsync();
                if (dh.MaTTDH == "TTDH0001") return true;
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> checkUpdate(string id)
        {
            try
            {
                var dh = db.DonHangs.Where(m => m.MaDH == id && m.ThanhToan==false && m.MaTTDH=="TTDH0001").Include(m=>m.CTDHs).SingleOrDefault();
                if(dh==null) return false;
                int s = await _tsService.getThoiGianThanhToan(id);
                if (s == 0)
                {
                    dh.MaTTDH = "TTDH0004";
                    foreach (var t in dh.CTDHs)
                    {
                        var tk = await db.TonKhos.Where(m => m.MaSP == t.MaSP).SingleOrDefaultAsync();
                        if (tk == null)
                        {
                            message = $"Sản phẩm {t.MaSP} không được xác định";
                            return false;
                        }
                        tk.SLDaBan -= t.SoLuong;
                        tk.SLCoTheBan += t.SoLuong;
                    }
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
            catch
            {
                message = "Đã xảy ra lỗi khi tự động cập nhật trạng thái đơn hàng.";
                return false;
            }
        } 

        public async Task<bool> CheckTraHang(string madonhang)
        {
            try
            {
                var dh = await db.DonHangs.Where(m => m.MaDH == madonhang).SingleOrDefaultAsync();
                var ycth = await db.DoiTraHangs.Where(m => m.MaDH == madonhang).SingleOrDefaultAsync();
                int time = await db.ThamSos.Where(m => m.MaTS == "TS000000").Select(m => m.ThoiGianTraHang).SingleOrDefaultAsync();
                if (ycth != null) return false;
                if (dh.MaTTDH == "TTDH0003" && (DateTime.Now - (dh.NgayNhanHang==null?DateTime.Now:dh.NgayNhanHang)).Value.TotalDays <= time) return true;
                return false;
            }
            catch
            {
                return false;
            }
        }
        
        
        public async Task<decimal> GetTongGiaTri(string madonhang)
        {
            try
            {
                var ctdh = await db.CTDHs.Where(m => m.MaDH == madonhang).ToListAsync();
                decimal ans = 0;
                foreach (var x in ctdh)
                {
                    ans += (x.DonGia * (decimal) x.SoLuong);
                }
                return ans;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<CTDonHang?> GetCTDonHang(string madonhang)
        {
            try
            {
                var dh = await db.DonHangs.Where(m => m.MaDH == madonhang).Include(m=>m.CTDHs).ThenInclude(m=>m.MaSPNavigation).ThenInclude(m=>m.MaNhanHieuNavigation).SingleOrDefaultAsync();
                CTDonHang ctdh = new CTDonHang();
                ctdh.MaDH = dh.MaDH;
                ctdh.NgayMuaHang = dh.NgayMuaHang;
                ctdh.MaKH = dh.MaKH;
                ctdh.MaKHNavigation = dh.MaKHNavigation;
                ctdh.LoaiDH = dh.LoaiDH;
                ctdh.CTDHs = dh.CTDHs;
                ctdh.MaPTTT = dh.MaPTTT;
                ctdh.MaTTDH = dh.MaTTDH;
                ctdh.MaPTTTNavigation = dh.MaPTTTNavigation;
                ctdh.MaTTDHNavigation = dh.MaTTDHNavigation;
                ctdh.MaNV = dh.MaNV;
                ctdh.PhiGiaoHang = dh.PhiGiaoHang;
                ctdh.NgayNhanHang = dh.NgayNhanHang;
                ctdh.ThanhToan = dh.ThanhToan;
                ctdh.TenNguoiNhan = dh.TenNguoiNhan;
                ctdh.SDTNguoiNhan = dh.SDTNguoiNhan;
                ctdh.DiaChiGiaoHang = dh.DiaChiGiaoHang;
                ctdh.GhiChu = dh.GhiChu;
                ctdh.DTH = await GetDTH(dh.MaDH);
                ctdh.HuyDon = await CheckHuyDon(dh.MaDH);
                ctdh.TraHang = await CheckTraHang(dh.MaDH);
                ctdh.TongGiaTri = await GetTongGiaTri(dh.MaDH);
                return ctdh;
            }
            catch
            {
                return null;
            }
        }

		//public async Task<KeyValuePair<int, decimal>> GetSLDHGT(DateTime time)
		//{
		//    try
		//    {
		//        // Lấy ngày hiện tại
		//        DateTime today = DateTime.Now.Date;

		//        // Lấy khoảng thời gian của 7 ngày vừa qua
		//        //DateTime startCurrentTime = today.AddDays(-6); // 7 ngày tính cả hôm nay
		//        //DateTime startPreviousTime = startCurrentTime.AddDays(-7); // 7 ngày trước đó
		//        DateTime startCurrentTime = time;
		//        DateTime startPreviousTime = time - (today - time);

		//        // Số lượng đơn hàng trong 7 ngày vừa qua
		//        int currentTimeOrders = await db.DonHangs
		//                                  .Where(dh => dh.NgayMuaHang >= startCurrentTime && dh.NgayMuaHang <= today)
		//                                  .CountAsync();

		//        // Số lượng đơn hàng trong 7 ngày trước đó
		//        int previousTimeOrders = await db.DonHangs
		//                                   .Where(dh => dh.NgayMuaHang >= startPreviousTime && dh.NgayMuaHang < startCurrentTime)
		//                                   .CountAsync();

		//        // Tính phần trăm tăng/giảm
		//        decimal percentageChange = 0;

		//        if (previousTimeOrders > 0)
		//        {
		//            percentageChange = ((decimal)(currentTimeOrders - previousTimeOrders) / previousTimeOrders) * 100;
		//        }
		//        else if (currentTimeOrders > 0)
		//        {
		//            // Nếu tuần trước không có đơn hàng và tuần hiện tại có đơn hàng, thì tăng 100%
		//            percentageChange = 100;
		//        }
		//        else
		//        {
		//            // Nếu cả hai tuần không có đơn hàng
		//            percentageChange = 0;
		//        }

		//        // Trả về kết quả dưới dạng cặp giá trị
		//        return new KeyValuePair<int, decimal>(currentTimeOrders, percentageChange);
		//    }
		//    catch (Exception ex)
		//    {
		//        // Xử lý ngoại lệ nếu xảy ra lỗi
		//        message = "lỗi: " + ex.Message;
		//        return new KeyValuePair<int, decimal>(0, 0);
		//    }
		//}

		public async Task<KeyValuePair<int, decimal>> GetSLDHGT(string field = "cd", string key = "week")
		{
			try
			{
                var lstime = getStartEndTime(field, key);
                DateTime startCurrentTime = lstime[0];
                DateTime endCurrentTime = lstime[1];
                DateTime startPreviousTime = lstime[2];
                DateTime endPreviousTime = lstime[3];

                int currentTimeOrders = await db.DonHangs
										  .Where(dh => dh.NgayMuaHang >= startCurrentTime && dh.NgayMuaHang <= endCurrentTime&& dh.MaTTDH=="TTDH0003")
										  .CountAsync();

				int previousTimeOrders = await db.DonHangs
										   .Where(dh => dh.NgayMuaHang >= startPreviousTime && dh.NgayMuaHang <= endPreviousTime&& dh.MaTTDH == "TTDH0003")
										   .CountAsync();

				
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
		public async Task<KeyValuePair<decimal, decimal>> GetSLTNGT(string field = "cd", string key = "week")
        {
            try
            {
                var lstime = getStartEndTime(field, key);
                DateTime startCurrentTime = lstime[0];
                DateTime endCurrentTime = lstime[1];
                DateTime startPreviousTime = lstime[2];
                DateTime endPreviousTime = lstime[3];


                decimal currentTimeOrders = 0;
                decimal previousTimeOrders = 0;

                var dhs = await db.DonHangs.Where(dh => dh.NgayMuaHang >= startCurrentTime && dh.NgayMuaHang <= endCurrentTime && dh.MaTTDH=="TTDH0003").Select(m => m.MaDH).ToListAsync();
                foreach (var x in dhs)
                {
                    var temp = await db.CTDHs.Where(m => m.MaDH == x).Include(m=>m.MaSPNavigation).ToListAsync();
                    foreach (var y in temp)
                    {
                        currentTimeOrders += y.SoLuong * y.DonGia;
                        //currentTimeOrders += y.SoLuong * (y.DonGia-y.MaSPNavigation.GiaNhap);
                    }

                }
                
                dhs = await db.DonHangs.Where(kh => kh.NgayMuaHang >= startPreviousTime && kh.NgayMuaHang <= endPreviousTime && kh.MaTTDH == "TTDH0003").Select(m => m.MaDH).ToListAsync();
                foreach (var x in dhs)
                {
                    var temp = await db.CTDHs.Where(m => m.MaDH == x).Include(m=>m.MaSPNavigation).ToListAsync();
                    foreach (var y in temp)
                    {
                        previousTimeOrders += y.SoLuong * y.DonGia;
                        //previousTimeOrders += y.SoLuong * (y.DonGia - y.MaSPNavigation.GiaNhap);
                    }
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

                return new KeyValuePair<decimal, decimal>(currentTimeOrders, percentageChange);
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu xảy ra lỗi
                message = "lỗi: " + ex.Message;
                return new KeyValuePair<decimal, decimal>(0, 0);
            }
        }


        public async Task<List<CTDonHang>> GetDonHangTrangThai(string mattdh)
        {
            try
            {
                List<CTDonHang> ans = new List<CTDonHang>();
                var dh = await db.DonHangs.Where(m => m.MaTTDH == mattdh).ToListAsync();
                foreach (var y in dh)
                {
                    CTDonHang? x = await GetCTDonHang(y.MaDH);
                    if (x != null)
                    {
                        ans.Add(x);
                    }
                }
                return ans;
            }
            catch
            {
                return new List<CTDonHang>();
            }
        }
        public async Task<List<CTDonHang>> GetDonHangCanXuLy()
        {
            try
            {
                List<CTDonHang> ans = new List<CTDonHang>();
                var dh = db.DonHangs.Where(m => m.MaTTDH == "TTDH0001").ToList();
                foreach (var y in dh)
                {
                    CTDonHang? x = await GetCTDonHang(y.MaDH);
                    if (x != null)
                    {
                        ans.Add(x);
                    }
                }
                return ans;
            }
            catch
            {
                return new List<CTDonHang>();
            }
        }
        


        //public List<CTDonHang> GetDonHangDangGiao(string manhanvien)
        //{
        //    try
        //    {
        //        List<CTDonHang> ans = new List<CTDonHang>();
        //        var dh = db.DonHangs.Where(m => m.MaNhanVienGiao == manhanvien && (m.MaTTDH == "TTDH0003" || m.MaTTDH == "TTDH0011")).ToList();
        //        foreach (var y in dh)
        //        {
        //            CTDonHang x = GetCTDonHang(y.MaDonHang);
        //            ans.Add(x);
        //        }
        //        return ans;
        //    }
        //    catch
        //    {
        //        return new List<CTDonHang>();
        //    }
        //}

        public async Task<bool> UpdateDonHang(string madonhang, string manhanvien, string mattdh)
        {
            try
            {
                if (string.IsNullOrEmpty(madonhang))
                {
                    message = "Mã đơn hàng không được để trống";
                    return false;
                }
                if (string.IsNullOrEmpty(manhanvien))
                {
                    message = "Mã nhân viên không được để trống";
                    return false;
                }
                if (string.IsNullOrEmpty(mattdh))
                {
                    message = "Trạng thái đơn hàng không được để trống";
                    return false;
                }
                var dh = await db.DonHangs.Where(m => m.MaDH == madonhang).Include(m=>m.CTDHs).SingleOrDefaultAsync();
                dh.MaTTDH = mattdh;
                if (mattdh == "TTDH0002")
                {
                    dh.MaNV = manhanvien;
                }
                if (mattdh == "TTDH0003")
                {
                    dh.NgayNhanHang = DateTime.Now;
                    foreach (var t in dh.CTDHs)
                    {
                        var tk = await db.TonKhos.Where(m => m.MaSP == t.MaSP).SingleOrDefaultAsync();
                        tk.SLTrongKho -= t.SoLuong;
                    }

                }
                db.SaveChanges();
                return true;
            }
            catch
            {
                message = "Đã có lỗi";
                return false;
            }
        }

        public async Task<bool> TaoYCTH(DoiTraHang model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.MaDH))
                {
                    message = "Mã đơn hàng bị trống";
                    return false;
                }
                if (string.IsNullOrEmpty(model.LiDoDT))
                {
                    message = "Lý do trả hàng không được để trống";
                    return false;
                }
                
                model.MaDT = await GetnewIDTraHang();
                model.NgayDT = DateTime.Now;
                model.TrangThai = false;
                db.DoiTraHangs.Add(model);
                db.SaveChanges();
                return true;
            }
            catch
            {
                message = "Đã có lỗi";
                return false;
            }
        }
        
        public async Task<CTTraHang?> GetCTYCTH(string matrahang)
        {
            try
            {
                var th = await db.DoiTraHangs.Where(m => m.MaDT == matrahang).SingleOrDefaultAsync();
                CTTraHang ctdh = new CTTraHang();
                ctdh.MaDH = th.MaDH;
                ctdh.MaDT = th.MaDT;
                ctdh.TrangThai = th.TrangThai;
                ctdh.LiDoDT = th.LiDoDT;
                ctdh.NgayXuLy = th.NgayXuLy;
                ctdh.NgayDT = th.NgayDT;
                ctdh.MaNV = th.MaNV;
                ctdh.MaDHNavigation = th.MaDHNavigation;
                ctdh.CTDonHang = await GetCTDonHang(th.MaDH);
                return ctdh;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<CTTraHang>?> GetDanhSachYCTH(bool? trangthai = null)
        {
            try
            {
                List<CTTraHang> ans = new List<CTTraHang>();
                if (trangthai==null)
                {
                    foreach (var x in db.DoiTraHangs)
                    {
                        CTTraHang? k = await GetCTYCTH(x.MaDT);
                        if (k != null)
                        {
                            ans.Add(k);
                        }
                    }
                    return ans;

                }
                else
                {
                    var th = db.DoiTraHangs.Where(m => m.TrangThai == trangthai).ToList();
                    foreach (var x in th)
                    {
                        CTTraHang? k = await GetCTYCTH(x.MaDT);
                        if (k != null)
                        {
                            ans.Add(k);
                        }
                    }
                    return ans;
                }
                
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UpdateYCTH(string matrahang, string manhanvien, bool trangthai)
        {
            try
            {
                if (string.IsNullOrEmpty(matrahang))
                {
                    message = "Mã trả hàng không được để trống";
                    return false;
                }
                if (string.IsNullOrEmpty(manhanvien))
                {
                    message = "Mã nhân viên không được để trống";
                    return false;
                }
                var th = await db.DoiTraHangs.Where(m => m.MaDT == matrahang).SingleOrDefaultAsync();
                th.TrangThai = trangthai;
                th.NgayXuLy = DateTime.Now;
                th.MaNV = manhanvien;
                db.SaveChanges();
                return true;
            }
            catch
            {
                message = "Đã có lỗi";
                return false;
            }
        
        }

        public async Task<List<decimal>> getGetDTTLDH(string field = "cd",string key ="week")
        {
            try
            {
                var lstime = getStartEndTime(field, key);
                DateTime startCurrentTime = lstime[0];

                var tt = await db.DonHangs.Where(m => m.LoaiDH == true && m.NgayMuaHang >= startCurrentTime && m.MaTTDH=="TTDH0003").Include(m=>m.CTDHs).ThenInclude(m=>m.MaSPNavigation).ToListAsync();
                decimal dttt = 0;
                foreach (var t in tt) { 
                    foreach(var ct in t.CTDHs)
                    {
                        //dttt += ct.SoLuong * ct.DonGia;
                        dttt += ct.SoLuong * (ct.DonGia - ct.MaSPNavigation.GiaNhap);
                    }
                }
                var online = await db.DonHangs.Where(m => m.LoaiDH == false && m.NgayMuaHang >= startCurrentTime && m.MaTTDH == "TTDH0003").Include(m => m.CTDHs).ThenInclude(m => m.MaSPNavigation).ToListAsync();
                decimal dtol = 0;
				foreach (var t in online)
				{
					foreach (var ct in t.CTDHs)
					{
						dtol += ct.SoLuong * (ct.DonGia - ct.MaSPNavigation.GiaNhap);
						//dtol += ct.SoLuong * ct.DonGia;
					}
				}
				return [dttt, dtol];
            }
            catch
            {
                return [0,0];
            }
        }
        
        public async Task<List<KeyValuePair<string, int>>> getThongKeSLDonHang(string key = "week")
        {
            try
            {
                var lt = getKeyTime(key);
                List<KeyValuePair<string, int>> ans = new List<KeyValuePair<string, int>>();
                for (int i = 0; i < lt.Count; i++)
                {
                    string lable = lt[i].Key;
                    int sl = 0;
                    if (i != lt.Count - 1)
                    {
                        sl = await db.DonHangs.Where(m => m.MaTTDH == "TTDH0003" && m.NgayMuaHang >= lt[i].Value && m.NgayMuaHang < lt[i + 1].Value).CountAsync();
                    }
                    else
                    {
                        sl = await db.DonHangs.Where(m => m.MaTTDH == "TTDH0003" && m.NgayMuaHang >= lt[i].Value).CountAsync();
                    }
                    ans.Add(new KeyValuePair<string, int>(lable, sl));
                }
                return ans;
            }
            catch
            {
                return [];
            }
        }

        public async Task<decimal> getDoanhThu(string maDH)
        {
            try
            {
                decimal tongDT = 0;
                var ctdhs = await db.CTDHs.Where(m=>m.MaDH==maDH).Include(m=>m.MaSPNavigation).ToListAsync();
                foreach(var x in ctdhs)
                {
                    tongDT += x.SoLuong * x.DonGia;
                    //tongDT += x.SoLuong * (x.DonGia-x.MaSPNavigation.GiaNhap);
                }
                return tongDT;
            }
            catch
            {
                return 0;
            }
        }
        public async Task<decimal> getLoiNhuan(string maDH)
        {
            try
            {
                decimal tongDT = 0;
                var ctdhs = await db.CTDHs.Where(m => m.MaDH == maDH).Include(m => m.MaSPNavigation).ToListAsync();
                foreach (var x in ctdhs)
                {
                    tongDT += x.SoLuong * (x.DonGia-x.MaSPNavigation.GiaNhap);
                }
                return tongDT;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<List<KeyValuePair<string, int>>> getThongKeDoanhThu(string key = "week")
        {
            try
            {
                var lt = getKeyTime(key);
                List<KeyValuePair<string, int>> ans = new List<KeyValuePair<string, int>>();
                for (int i = 0; i < lt.Count; i++)
                {
                    string lable = lt[i].Key;
                    int sl = 0;
                    if (i != lt.Count - 1)
                    {
                        var dhs = await db.DonHangs.Where(m => m.MaTTDH == "TTDH0003" && m.NgayMuaHang >= lt[i].Value && m.NgayMuaHang < lt[i + 1].Value).Select(m=>m.MaDH).ToListAsync();
                        foreach(var dh in dhs)
                        {
                            sl += (int) await getDoanhThu(dh);
                        }
                    }
                    else
                    {
                        var dhs = await db.DonHangs.Where(m => m.MaTTDH == "TTDH0003" && m.NgayMuaHang >= lt[i].Value).Select(m => m.MaDH).ToListAsync();
                        foreach (var dh in dhs)
                        {
                            sl += (int)await getDoanhThu(dh);
                        }
                    }
                    ans.Add(new KeyValuePair<string, int>(lable, sl));
                }
                return ans;
            }
            catch
            {
                return [];
            }
        }
        public async Task<List<KeyValuePair<string, int>>> getThongKeLoiNhuan(string key = "week")
        {
            try
            {
                var lt = getKeyTime(key);
                List<KeyValuePair<string, int>> ans = new List<KeyValuePair<string, int>>();
                for (int i = 0; i < lt.Count; i++)
                {
                    string lable = lt[i].Key;
                    int sl = 0;
                    if (i != lt.Count - 1)
                    {
                        var dhs = await db.DonHangs.Where(m => m.MaTTDH == "TTDH0003" && m.NgayMuaHang >= lt[i].Value && m.NgayMuaHang < lt[i + 1].Value).Select(m => m.MaDH).ToListAsync();
                        foreach (var dh in dhs)
                        {
                            sl += (int)await getLoiNhuan(dh);
                        }
                    }
                    else
                    {
                        var dhs = await db.DonHangs.Where(m => m.MaTTDH == "TTDH0003" && m.NgayMuaHang >= lt[i].Value).Select(m => m.MaDH).ToListAsync();
                        foreach (var dh in dhs)
                        {
                            sl += (int)await getLoiNhuan(dh);
                        }
                    }
                    ans.Add(new KeyValuePair<string, int>(lable, sl));
                }
                return ans;
            }
            catch
            {
                return [];
            }
        }

        public async Task<List<DTheoThoiGian>> DoanhThuTheoThoiGian(DateTime startDate, DateTime endDate, string key = "day")
        {
            var filtered = db.CTDHs
                .Where(ct => ct.MaDHNavigation.NgayMuaHang >= startDate
                  && ct.MaDHNavigation.NgayMuaHang <= endDate
                  && (ct.MaDHNavigation.MaTTDH == "TTDH0003"
                   || ct.MaDHNavigation.MaTTDH == "TTDH0005")) 
                .Include(m=>m.MaDHNavigation).Include(m=>m.MaSPNavigation)
                .AsEnumerable();
            if (filtered == null) return new List<DTheoThoiGian>();
            if (key == "month")
            {
                return filtered.GroupBy(ct => new
                {
                    Nam = ct.MaDHNavigation.NgayMuaHang.Year,
                    Thang = ct.MaDHNavigation.NgayMuaHang.Month
                })
                .Select(g => new DTheoThoiGian
                {
                    ThoiGian = new DateTime(g.Key.Nam, g.Key.Thang,1),
                    TongSLDonHang = g.Select(x => x.MaDH).Distinct().Count(),
                    SLDonHangTra = g.Where(x => x.MaDHNavigation.MaTTDH == "TTDH0005").Select(x => x.MaDH).Distinct().Count(),
                    TongTienTraLai = g.Where(x => x.MaDHNavigation.MaTTDH == "TTDH0005").Sum(x => x.SoLuong * x.DonGia),
                    TongDoanhThu = g.Where(x => x.MaDHNavigation.MaTTDH == "TTDH0003").Sum(x => x.SoLuong * x.DonGia),
                    TongGiaVon = g.Where(x => x.MaDHNavigation.MaTTDH == "TTDH0003").Sum(x => x.SoLuong * x.MaSPNavigation.GiaNhap),
                    LoiNhuan = g.Where(x => x.MaDHNavigation.MaTTDH == "TTDH0003").Sum(x => x.SoLuong * x.DonGia) - g.Sum(x => x.SoLuong * x.MaSPNavigation.GiaNhap)
                })
                .OrderBy(x => x.ThoiGian)
                .ToList();
            }
            return filtered.GroupBy(ct => ct.MaDHNavigation.NgayMuaHang.Date)
                .Select(g => new DTheoThoiGian
                {
                    ThoiGian = g.Key,
                    TongSLDonHang = g.Select(x => x.MaDH).Distinct().Count(),
                    SLDonHangTra = g.Where(x => x.MaDHNavigation.MaTTDH == "TTDH0005").Select(x => x.MaDH).Distinct().Count(),
                    TongTienTraLai = g.Where(x => x.MaDHNavigation.MaTTDH == "TTDH0005").Sum(x => x.SoLuong * x.DonGia),
                    TongDoanhThu = g.Where(x => x.MaDHNavigation.MaTTDH == "TTDH0003").Sum(x => x.SoLuong * x.DonGia),
                    TongGiaVon = g.Where(x => x.MaDHNavigation.MaTTDH == "TTDH0003").Sum(x => x.SoLuong * x.MaSPNavigation.GiaNhap),
                    LoiNhuan = g.Where(x => x.MaDHNavigation.MaTTDH == "TTDH0003").Sum(x => x.SoLuong * x.DonGia) - g.Sum(x => x.SoLuong * x.MaSPNavigation.GiaNhap)
                })
                .OrderBy(x => x.ThoiGian)
                .ToList();
        }
        public async Task<List<DTTheoNhanHieu>> DoanhThuTheoNhanHieu(DateTime startDate, DateTime endDate)
        {
            var filtered = db.CTDHs
                .Where(ct => ct.MaDHNavigation.NgayMuaHang >= startDate
                  && ct.MaDHNavigation.NgayMuaHang <= endDate
                  && (ct.MaDHNavigation.MaTTDH == "TTDH0003"
                   || ct.MaDHNavigation.MaTTDH == "TTDH0005"))
                .Include(m => m.MaDHNavigation).Include(m => m.MaSPNavigation).ThenInclude(m=>m.MaNhanHieuNavigation)
                .AsEnumerable();
            if (filtered == null) return new List<DTTheoNhanHieu>();
            return filtered.GroupBy(ct => ct.MaSPNavigation.MaNhanHieuNavigation.TenNhanHieu)
                .Select(g => new DTTheoNhanHieu
                {
                    NhanHieu = g.Key,
                    TongSLDonHang = g.Select(x => x.MaDH).Distinct().Count(),
                    SLDonHangTra = g.Where(x => x.MaDHNavigation.MaTTDH == "TTDH0005").Select(x => x.MaDH).Distinct().Count(),
                    TongTienTraLai = g.Where(x => x.MaDHNavigation.MaTTDH == "TTDH0005").Sum(x => x.SoLuong * x.DonGia),
                    TongDoanhThu = g.Where(x => x.MaDHNavigation.MaTTDH == "TTDH0003").Sum(x => x.SoLuong * x.DonGia),
                    TongGiaVon = g.Where(x => x.MaDHNavigation.MaTTDH == "TTDH0003").Sum(x => x.SoLuong * x.MaSPNavigation.GiaNhap),
                    LoiNhuan = g.Where(x => x.MaDHNavigation.MaTTDH == "TTDH0003").Sum(x => x.SoLuong * x.DonGia) - g.Sum(x => x.SoLuong * x.MaSPNavigation.GiaNhap)
                })
                .OrderBy(x => x.NhanHieu)
                .ToList();
        }


    }
}
