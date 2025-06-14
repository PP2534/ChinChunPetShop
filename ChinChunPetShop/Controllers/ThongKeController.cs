using ClosedXML.Excel;
using ChinChunPetShop.Models.Configs;
using ChinChunPetShop.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChinChunPetShop.Models.Entity;
using DocumentFormat.OpenXml.InkML;
using ChinChunPetShop.Models.ViewModel;

namespace ChinChunPetShop.Controllers
{
    public class ThongKeController : BaseController
    {
        private readonly DonHangService _dhService;
        public ThongKeController(SessionConfig session, DonHangService dhService) : base(session)
        {
            _dhService = dhService;
        }

        [Authorize(Policy = "Admin")]
        [Route("admin/baocao/theothoigian")]
        public async Task<IActionResult> DoanhThuTheoThoiGian()
        {
            var user = _session.getNhanVien();
            if (user == null)
            {
                return Redirect("~/nhanvien/dangnhap?next=~/admin/baocao/theothoigian");
            }
            ViewBag.from = new DateTime(2025, 01, 1).ToString("dd-MM-yyyy"); 
            ViewBag.to = DateTime.Now.ToString("dd-MM-yyyy");
            ViewBag.key = "day";
            ViewBag.rkey = "ngày";
            ViewBag.format = "dd/MM/yyyy";
            ViewBag.format_header = "[Tháng] MM [năm] yyyy";
            var model = await _dhService.DoanhThuTheoThoiGian(new DateTime(2025, 01, 01), DateTime.Now);
            return View(model);
        }
        [Authorize(Policy = "Admin")]
        [Route("admin/baocao/theothoigian")]
        [HttpPost]
        public async Task<IActionResult> DoanhThuTheoThoiGian(string from, string to, string key="day")
        {
            var user = _session.getNhanVien();
            if (user == null)
            {
                return Redirect("~/nhanvien/dangnhap?next=~/admin/baocao/theothoigian");
            }

            if (from == null)
            {
                from = new DateTime(2025, 01, 01).ToString("dd-MM-yyyy");
            }
            if (to == null)
            {
                to = DateTime.Now.ToString("dd-MM-yyyy");
            }
            ViewBag.format = "dd/MM/yyyy";
            ViewBag.format_header = "[Tháng] MM [năm] yyyy";
            ViewBag.rkey = "ngày";
            ViewBag.key = key;
            ViewBag.from = from; ViewBag.to = to;
            var start = DateTime.Parse(from);
            var end = DateTime.Parse(to).AddDays(1).AddHours(-1);
            if (key == "month")
            {
                ViewBag.rkey = "tháng";
                ViewBag.format = "MM/yyyy";
                ViewBag.format_header = "[Năm] yyyy";
                end = new DateTime(end.Year, end.Month, 1);
                end = end.AddMonths(1).AddDays(-1);
            }
            var model = await _dhService.DoanhThuTheoThoiGian(start, end, key);
            return View(model);
        }
        [Authorize(Policy = "Admin")]
        [Route("admin/baocao/theothoigian/xuatexcel")]
        [HttpPost]
        public async Task<IActionResult> DoanhThuTheoThoiGianExportToExcel(string to, string from, string key = "day")
        {
            var user = _session.getNhanVien();
            if (user == null)
            {
                return Redirect("~/nhanvien/dangnhap?next=~/admin/baocao/theothoigian");
            }
            if (from == null)
            {
                from = new DateTime(2025, 01, 01).ToString("dd-MM-yyyy");
            }
            if (to == null)
            {
                to = DateTime.Now.ToString("dd-MM-yyyy");
            }
            var start = DateTime.Parse(from);
            var end = DateTime.Parse(to).AddDays(1).AddHours(-1);
            string rkey = "ngay";
            if (key == "month")
            {
                rkey = "thang";
                end = new DateTime(end.Year, end.Month, 1);
                end = end.AddMonths(1).AddDays(-1);
            }
            var model = await _dhService.DoanhThuTheoThoiGian(start, end, key);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DoanhThuTheoThoiGian_" + rkey);
                worksheet.Style.Font.FontName = "Times New Roman";
                worksheet.Style.Font.FontSize = 12;
                //worksheet.Cell(1, 1).Value = "STT";
                worksheet.Cell(1, 1).Value = key == "day" ?"Ngày":"Tháng";
                worksheet.Cell(1, 2).Value = "Tổng số đơn hàng";
                worksheet.Cell(1, 3).Value = "Số đơn hàng đã trả";
                worksheet.Cell(1, 4).Value = "Tổng tiền đã trả lại";
                worksheet.Cell(1, 5).Value = "Tổng doanh thu";
                worksheet.Cell(1, 6).Value = "Tổng giá vốn";
                worksheet.Cell(1, 7).Value = "Lợi nhuận";

                int row = 2;
                //int stt = 1;
                foreach (var item in model)
                {
                    //worksheet.Cell(row, 1).Value = stt++;
                    worksheet.Cell(row, 1).Value = key=="day"? item.ThoiGian.ToString("dd/MM/yyyy"): item.ThoiGian.ToString("MM/yyyy");
                    worksheet.Cell(row, 2).Value = item.TongSLDonHang;
                    worksheet.Cell(row, 3).Value = item.SLDonHangTra;
                    worksheet.Cell(row, 4).Value = item.TongTienTraLai;
                    worksheet.Cell(row, 5).Value = item.TongDoanhThu;
                    worksheet.Cell(row, 6).Value = item.TongGiaVon;
                    worksheet.Cell(row, 7).Value = item.LoiNhuan;

                    row++;
                }
                worksheet.Cell(row, 1).Value = "Tổng";
                worksheet.Cell(row, 2).Value = model.Sum(m=>m.TongSLDonHang);
                worksheet.Cell(row, 3).Value = model.Sum(m=>m.SLDonHangTra);
                worksheet.Cell(row, 4).Value = model.Sum(m=>m.TongTienTraLai);
                worksheet.Cell(row, 5).Value = model.Sum(m => m.TongDoanhThu);
                worksheet.Cell(row, 6).Value = model.Sum(m => m.TongGiaVon);
                worksheet.Cell(row, 7).Value = model.Sum(m => m.LoiNhuan);

                //Định dạng
                worksheet.Range("A1:G1").Style.Font.SetBold(true);
                worksheet.Range($"A{row}:G{row}").Style.Font.SetBold(true);
                worksheet.Range($"A1:G{row}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range($"A1:G{row}").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                //worksheet.Range($"A1:A{row}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                //worksheet.Range($"A1:A{row}").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Range($"A1:G{row}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range($"A1:G{row}").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Range($"D2:G{row}").Style.NumberFormat.Format = "#,##0 \"₫\"";
                // 5. Format: tự động điều chỉnh độ rộng cột
                worksheet.Columns().AdjustToContents();

                // 6. Xuất ra MemoryStream
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    // 7. Trả về file với content-type Excel
                    string fileName = $"DoanhThu_TheoThoiGian_{to}_{from}.xlsx";
                    return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                fileName);
                }
            }
        }
        [Authorize(Policy = "Admin")]
        [Route("admin/baocao/theonhanhieu")]
        public async Task<IActionResult> DoanhThuTheoNhanHieu()
        {
            var user = _session.getNhanVien();
            if (user == null)
            {
                return Redirect("~/nhanvien/dangnhap?next=~/admin/baocao/theonhanhieu");
            }
            ViewBag.from = new DateTime(2025, 01, 1).ToString("dd-MM-yyyy");
            ViewBag.to = DateTime.Now.ToString("dd-MM-yyyy");
            ViewBag.key = "day";
            ViewBag.format = "dd/MM/yyyy";
            ViewBag.format_header = "[Tháng] MM [năm] yyyy";
            var model = await _dhService.DoanhThuTheoNhanHieu(new DateTime(2025, 01, 01), DateTime.Now);
            return View(model);
        }
        [Authorize(Policy = "Admin")]
        [Route("admin/baocao/theonhanhieu")]
        [HttpPost]
        public async Task<IActionResult> DoanhThuTheoNhanHieu(string from, string to)
        {
            var user = _session.getNhanVien();
            if (user == null)
            {
                return Redirect("~/nhanvien/dangnhap?next=~/admin/baocao/theonhanhieu");
            }

            if (from == null)
            {
                from = new DateTime(2025, 01, 01).ToString("dd-MM-yyyy");
            }
            if (to == null)
            {
                to = DateTime.Now.ToString("dd-MM-yyyy");
            }
            ViewBag.format = "dd/MM/yyyy";
            ViewBag.format_header = "[Tháng] MM [năm] yyyy";
            ViewBag.from = from; ViewBag.to = to;
            var start = DateTime.Parse(from);
            var end = DateTime.Parse(to).AddDays(1).AddHours(-1);
            var model = await _dhService.DoanhThuTheoNhanHieu(start, end);
            return View(model);
        }
        [Authorize(Policy = "Admin")]
        [Route("admin/baocao/theonhanhieu/xuatexcel")]
        [HttpPost]
        public async Task<IActionResult> DoanhThuTheoNhanHieuExportToExcel(string to, string from)
        {
            var user = _session.getNhanVien();
            if (user == null)
            {
                return Redirect("~/nhanvien/dangnhap?next=~/admin/baocao/theonhanhieu");
            }
            if (from == null)
            {
                from = new DateTime(2025, 01, 01).ToString("dd-MM-yyyy");
            }
            if (to == null)
            {
                to = DateTime.Now.ToString("dd-MM-yyyy");
            }
            var start = DateTime.Parse(from);
            var end = DateTime.Parse(to).AddDays(1).AddHours(-1);

            var model = await _dhService.DoanhThuTheoNhanHieu(start, end);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DoanhThuTheoNhanHieu");
                worksheet.Style.Font.FontName = "Times New Roman";
                worksheet.Style.Font.FontSize = 12;
                worksheet.Cell(1, 1).Value = "Nhãn hiệu";
                worksheet.Cell(1, 2).Value = "Tổng số đơn hàng";
                worksheet.Cell(1, 3).Value = "Số đơn hàng đã trả";
                worksheet.Cell(1, 4).Value = "Tổng tiền đã trả lại";
                worksheet.Cell(1, 5).Value = "Tổng doanh thu";
                worksheet.Cell(1, 6).Value = "Tổng giá vốn";
                worksheet.Cell(1, 7).Value = "Lợi nhuận";

                int row = 2;
                foreach (var item in model)
                {
                    worksheet.Cell(row, 1).Value = item.NhanHieu;
                    worksheet.Cell(row, 2).Value = item.TongSLDonHang;
                    worksheet.Cell(row, 3).Value = item.SLDonHangTra;
                    worksheet.Cell(row, 4).Value = item.TongTienTraLai;
                    worksheet.Cell(row, 5).Value = item.TongDoanhThu;
                    worksheet.Cell(row, 6).Value = item.TongGiaVon;
                    worksheet.Cell(row, 7).Value = item.LoiNhuan;

                    row++;
                }
                worksheet.Cell(row, 1).Value = "Tổng";
                worksheet.Cell(row, 2).Value = model.Sum(m => m.TongSLDonHang);
                worksheet.Cell(row, 3).Value = model.Sum(m => m.SLDonHangTra);
                worksheet.Cell(row, 4).Value = model.Sum(m => m.TongTienTraLai);
                worksheet.Cell(row, 5).Value = model.Sum(m => m.TongDoanhThu);
                worksheet.Cell(row, 6).Value = model.Sum(m => m.TongGiaVon);
                worksheet.Cell(row, 7).Value = model.Sum(m => m.LoiNhuan);

                //Định dạng
                worksheet.Range("A1:G1").Style.Font.SetBold(true);
                worksheet.Range($"A{row}:G{row}").Style.Font.SetBold(true);
                worksheet.Range($"A1:G{row}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range($"A1:G{row}").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range($"B1:G{row}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range($"B1:G{row}").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Range($"D2:G{row}").Style.NumberFormat.Format = "#,##0 \"₫\"";
                // 5. Format: tự động điều chỉnh độ rộng cột
                worksheet.Columns().AdjustToContents();

                // 6. Xuất ra MemoryStream
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    // 7. Trả về file với content-type Excel
                    string fileName = $"DoanhThu_TheoNhanHieu_{to}_{from}.xlsx";
                    return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                fileName);
                }
            }
        }
        [Authorize(Policy = "Admin")]
        [Route("admin/baocao/xuatbanin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XuatBanIn(string loaiBaoCao, string from, string to, string key = "day")
        {
            var user = _session.getNhanVien();
            if (user == null)
            {
                return Redirect("~/nhanvien/dangnhap?next=~/admin");
            }
            ViewBag.NhanVien = user.HoNV + " " + user.TenNV;
            switch (loaiBaoCao)
            {
                case "DoanhThuTheoThoiGian":
                    if (from == null)
                    {
                        from = new DateTime(2025, 01, 01).ToString("dd-MM-yyyy");
                    }
                    if (to == null)
                    {
                        to = DateTime.Now.ToString("dd-MM-yyyy");
                    }
                    var start = DateTime.Parse(from);
                    var end = DateTime.Parse(to).AddDays(1).AddHours(-1);
                    string rkey = "ngay";
                    if (key == "month")
                    {
                        rkey = "thang";
                        end = new DateTime(end.Year, end.Month, 1);
                        end = end.AddMonths(1).AddDays(-1);
                    }
                    var model = await _dhService.DoanhThuTheoThoiGian(start, end, key);

                    ViewBag.ReportTitle = $"Báo cáo doanh thu theo thời gian từ {from} đến hết {to}";
                    ViewBag.key = key;
                    return View(new BanIn
                    {
                        loaiBaoCao = loaiBaoCao,
                        DTheoThoiGian = model,
                    });

                case "DoanhThuTheoNhanHieu":
                    if (from == null)
                    {
                        from = new DateTime(2025, 01, 01).ToString("dd-MM-yyyy");
                    }
                    if (to == null)
                    {
                        to = DateTime.Now.ToString("dd-MM-yyyy");
                    }
                    start = DateTime.Parse(from);
                    end = DateTime.Parse(to).AddDays(1).AddHours(-1);

                    var model_nhanhieu = await _dhService.DoanhThuTheoNhanHieu(start, end);

                    ViewBag.ReportTitle = $"Báo cáo doanh thu theo nhãn hiệu từ {from} đến hết {to}";
                    return View(new BanIn
                    {
                        loaiBaoCao = loaiBaoCao,
                        DTheoNhanHieu = model_nhanhieu
                    });

                default:
                    return BadRequest("Loại báo cáo không hợp lệ");
            }
        }

    }
}
