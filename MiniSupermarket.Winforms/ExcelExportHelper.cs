using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace MiniSupermarket.WinForms
{
    /// <summary>
    /// Tiêu chí 48: Bộ xuất báo cáo tự động sang Excel (.xlsx) và CSV (.csv)
    /// Hỗ trợ định dạng màu sắc, kẻ khung bảng, định dạng số tiền, ngày tháng và tự động căn chỉnh độ rộng cột
    /// Sử dụng OpenXML thuần thông qua ZipArchive có sẵn trong .NET, không phụ thuộc thư viện bên ngoài.
    /// </summary>
    public static class ExcelExportHelper
    {
        private static string EscXml(string? text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        private static string EscCsv(string? text)
        {
            if (text == null) return "\"\"";
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }

        #region XUẤT BÁO CÁO SẢN PHẨM & TỒN KHO

        public static void ExportProducts(string filePath, IEnumerable<ProductDto> products, string cashierName)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (ext == ".csv")
            {
                ExportProductsCsv(filePath, products, cashierName);
            }
            else
            {
                ExportProductsExcel(filePath, products, cashierName);
            }
        }

        private static void ExportProductsCsv(string filePath, IEnumerable<ProductDto> products, string cashierName)
        {
            var sb = new StringBuilder();

            sb.AppendLine("HỆ THỐNG SIÊU THỊ MINI PURE FOOD");
            sb.AppendLine("BÁO CÁO DANH SÁCH SẢN PHẨM & TỒN KHO");
            sb.AppendLine($"Thời gian xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss},Người lập: {cashierName}");
            sb.AppendLine();
            sb.AppendLine("STT,Mã SP,Mã vạch,Tên sản phẩm,Nhóm hàng,Đơn giá bán (VNĐ),Số lượng tồn,Trạng thái tồn kho");

            int stt = 1;
            decimal totalStockValue = 0;
            int totalStockQty = 0;

            foreach (var p in products)
            {
                string status = p.StockQuantity <= 0 ? "HẾT HÀNG" : (p.StockQuantity <= 10 ? "SẮP HẾT" : "CÒN HÀNG");
                totalStockQty += p.StockQuantity;
                totalStockValue += p.Price * p.StockQuantity;

                sb.AppendLine(string.Join(",",
                    stt++,
                    p.ProductId,
                    EscCsv(p.Barcode),
                    EscCsv(p.ProductName),
                    EscCsv(p.CategoryName),
                    p.Price.ToString("0"),
                    p.StockQuantity,
                    EscCsv(status)
                ));
            }

            sb.AppendLine();
            sb.AppendLine($"TỔNG CỘNG SỐ MẶT HÀNG,{stt - 1} sản phẩm");
            sb.AppendLine($"TỔNG SỐ LƯỢNG TỒN KHO,{totalStockQty} đơn vị");
            sb.AppendLine($"TỔNG GIÁ TRỊ TỒN KHO,{totalStockValue:0} VNĐ");

            File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
        }

        private static void ExportProductsExcel(string filePath, IEnumerable<ProductDto> products, string cashierName)
        {
            var sheetSb = new StringBuilder();
            sheetSb.Append(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<worksheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">
  <cols>
    <col min=""1"" max=""1"" width=""7"" customWidth=""1""/>
    <col min=""2"" max=""2"" width=""10"" customWidth=""1""/>
    <col min=""3"" max=""3"" width=""18"" customWidth=""1""/>
    <col min=""4"" max=""4"" width=""34"" customWidth=""1""/>
    <col min=""5"" max=""5"" width=""20"" customWidth=""1""/>
    <col min=""6"" max=""6"" width=""18"" customWidth=""1""/>
    <col min=""7"" max=""7"" width=""16"" customWidth=""1""/>
    <col min=""8"" max=""8"" width=""18"" customWidth=""1""/>
  </cols>
  <sheetData>
    <row r=""1"" ht=""28"" customHeight=""1"">
      <c r=""A1"" s=""1"" t=""inlineStr""><is><t>HỆ THỐNG SIÊU THỊ MINI PURE FOOD</t></is></c>
    </row>
    <row r=""2"" ht=""24"" customHeight=""1"">
      <c r=""A2"" s=""2"" t=""inlineStr""><is><t>BÁO CÁO DANH SÁCH SẢN PHẨM &amp; TỒN KHO</t></is></c>
    </row>
    <row r=""3"" ht=""18"" customHeight=""1"">
      <c r=""A3"" s=""3"" t=""inlineStr""><is><t>Thời gian xuất: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "  |  Người lập báo cáo: " + EscXml(cashierName) + @"</t></is></c>
    </row>
    <row r=""4""/>
    <row r=""5"" ht=""26"" customHeight=""1"">
      <c r=""A5"" s=""5"" t=""inlineStr""><is><t>STT</t></is></c>
      <c r=""B5"" s=""5"" t=""inlineStr""><is><t>Mã SP</t></is></c>
      <c r=""C5"" s=""5"" t=""inlineStr""><is><t>Mã vạch</t></is></c>
      <c r=""D5"" s=""5"" t=""inlineStr""><is><t>Tên sản phẩm</t></is></c>
      <c r=""E5"" s=""5"" t=""inlineStr""><is><t>Nhóm hàng</t></is></c>
      <c r=""F5"" s=""5"" t=""inlineStr""><is><t>Đơn giá (VNĐ)</t></is></c>
      <c r=""G5"" s=""5"" t=""inlineStr""><is><t>Tồn kho</t></is></c>
      <c r=""H5"" s=""5"" t=""inlineStr""><is><t>Trạng thái tồn</t></is></c>
    </row>
");

            int rowIdx = 6;
            int stt = 1;
            int totalStockQty = 0;
            decimal totalStockValue = 0;

            foreach (var p in products)
            {
                bool isZebra = (stt % 2 == 0);
                bool isLowStock = p.StockQuantity <= 10;
                string status = p.StockQuantity <= 0 ? "HẾT HÀNG" : (isLowStock ? "SẮP HẾT" : "CÒN HÀNG");

                totalStockQty += p.StockQuantity;
                totalStockValue += p.Price * p.StockQuantity;

                int sText = isZebra ? 9 : 6;
                int sCenter = isZebra ? 10 : 7;
                int sNum = isZebra ? 11 : 8;

                int sStock = isLowStock ? 12 : sNum;
                int sStatus = isLowStock ? 13 : sCenter;

                sheetSb.Append($@"    <row r=""{rowIdx}"" ht=""21"" customHeight=""1"">
      <c r=""A{rowIdx}"" s=""{sCenter}""><v>{stt}</v></c>
      <c r=""B{rowIdx}"" s=""{sCenter}""><v>{p.ProductId}</v></c>
      <c r=""C{rowIdx}"" s=""{sCenter}"" t=""inlineStr""><is><t>{EscXml(p.Barcode)}</t></is></c>
      <c r=""D{rowIdx}"" s=""{sText}"" t=""inlineStr""><is><t>{EscXml(p.ProductName)}</t></is></c>
      <c r=""E{rowIdx}"" s=""{sText}"" t=""inlineStr""><is><t>{EscXml(p.CategoryName)}</t></is></c>
      <c r=""F{rowIdx}"" s=""{sNum}""><v>{(long)p.Price}</v></c>
      <c r=""G{rowIdx}"" s=""{sStock}""><v>{p.StockQuantity}</v></c>
      <c r=""H{rowIdx}"" s=""{sStatus}"" t=""inlineStr""><is><t>{status}</t></is></c>
    </row>
");
                rowIdx++;
                stt++;
            }

            int count = stt - 1;
            sheetSb.Append($@"    <row r=""{rowIdx}"" ht=""25"" customHeight=""1"">
      <c r=""A{rowIdx}"" s=""14"" t=""inlineStr""><is><t>TỔNG CỘNG</t></is></c>
      <c r=""B{rowIdx}"" s=""14""/>
      <c r=""C{rowIdx}"" s=""14""/>
      <c r=""D{rowIdx}"" s=""14"" t=""inlineStr""><is><t>{count} mặt hàng</t></is></c>
      <c r=""E{rowIdx}"" s=""14""/>
      <c r=""F{rowIdx}"" s=""15""><v>{(long)totalStockValue}</v></c>
      <c r=""G{rowIdx}"" s=""15""><v>{totalStockQty}</v></c>
      <c r=""H{rowIdx}"" s=""14""/>
    </row>
  </sheetData>
  <mergeCells count=""4"">
    <mergeCell ref=""A1:H1""/>
    <mergeCell ref=""A2:H2""/>
    <mergeCell ref=""A3:H3""/>
    <mergeCell ref=""A{rowIdx}:C{rowIdx}""/>
  </mergeCells>
</worksheet>");

            BuildXlsxPackage(filePath, "SanPham_TonKho", sheetSb.ToString());
        }

        #endregion

        #region XUẤT BÁO CÁO DOANH THU & ĐƠN HÀNG

        public static void ExportRevenue(string filePath, ReportDto data, DateTime from, DateTime to, string cashierName)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (ext == ".csv")
            {
                ExportRevenueCsv(filePath, data, from, to, cashierName);
            }
            else
            {
                ExportRevenueExcel(filePath, data, from, to, cashierName);
            }
        }

        private static void ExportRevenueCsv(string filePath, ReportDto data, DateTime from, DateTime to, string cashierName)
        {
            var sb = new StringBuilder();

            sb.AppendLine("HỆ THỐNG SIÊU THỊ MINI PURE FOOD");
            sb.AppendLine("BÁO CÁO DOANH THU BÁN HÀNG THEO THỜI GIAN");
            sb.AppendLine($"Khoảng thời gian: Từ {from:dd/MM/yyyy} đến {to:dd/MM/yyyy},Ngày lập: {DateTime.Now:dd/MM/yyyy HH:mm:ss},Người lập: {cashierName}");
            sb.AppendLine();
            sb.AppendLine("--- THỐNG KÊ TỔNG QUAN ---");
            sb.AppendLine($"Tổng doanh thu,{data.Summary.TotalRevenue:0} VNĐ");
            sb.AppendLine($"Tổng số đơn hàng,{data.Summary.TotalOrders} đơn");
            sb.AppendLine($"Giá trị trung bình/đơn,{data.Summary.AverageOrderValue:0} VNĐ");
            sb.AppendLine($"Tổng tiền giảm giá (Điểm thưởng),{data.Summary.TotalDiscount:0} VNĐ");
            sb.AppendLine();

            sb.AppendLine("--- DANH SÁCH CHI TIẾT HÓA ĐƠN ---");
            sb.AppendLine("STT,Mã đơn hàng,Thời gian,Thu ngân,Khách hàng,Hình thức thanh toán,Giảm giá (VNĐ),Thành tiền (VNĐ)");

            int stt = 1;
            foreach (var o in data.Orders)
            {
                string customer = string.IsNullOrWhiteSpace(o.CustomerName) ? "Khách vãng lai" : o.CustomerName;
                string method = o.PaymentMethod == "Cash" ? "Tiền mặt" : (o.PaymentMethod == "Card" ? "Thẻ / Chuyển khoản" : (o.PaymentMethod ?? "Khác"));

                sb.AppendLine(string.Join(",",
                    stt++,
                    EscCsv(o.OrderCode),
                    EscCsv(o.CreatedAt.ToString("dd/MM/yyyy HH:mm")),
                    EscCsv(o.CashierUsername),
                    EscCsv(customer),
                    EscCsv(method),
                    o.DiscountAmount.ToString("0"),
                    o.FinalAmount.ToString("0")
                ));
            }

            sb.AppendLine();
            sb.AppendLine($"TỔNG CỘNG DOANH THU,{data.Summary.TotalRevenue:0} VNĐ");

            File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
        }

        private static void ExportRevenueExcel(string filePath, ReportDto data, DateTime from, DateTime to, string cashierName)
        {
            var sheetSb = new StringBuilder();
            sheetSb.Append(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<worksheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">
  <cols>
    <col min=""1"" max=""1"" width=""8"" customWidth=""1""/>
    <col min=""2"" max=""2"" width=""18"" customWidth=""1""/>
    <col min=""3"" max=""3"" width=""18"" customWidth=""1""/>
    <col min=""4"" max=""4"" width=""16"" customWidth=""1""/>
    <col min=""5"" max=""5"" width=""24"" customWidth=""1""/>
    <col min=""6"" max=""6"" width=""20"" customWidth=""1""/>
    <col min=""7"" max=""7"" width=""18"" customWidth=""1""/>
    <col min=""8"" max=""8"" width=""22"" customWidth=""1""/>
  </cols>
  <sheetData>
    <row r=""1"" ht=""28"" customHeight=""1"">
      <c r=""A1"" s=""1"" t=""inlineStr""><is><t>HỆ THỐNG SIÊU THỊ MINI PURE FOOD</t></is></c>
    </row>
    <row r=""2"" ht=""24"" customHeight=""1"">
      <c r=""A2"" s=""2"" t=""inlineStr""><is><t>BÁO CÁO DOANH THU BÁN HÀNG THEO THỜI GIAN</t></is></c>
    </row>
    <row r=""3"" ht=""18"" customHeight=""1"">
      <c r=""A3"" s=""3"" t=""inlineStr""><is><t>Khoảng thời gian: Từ " + from.ToString("dd/MM/yyyy") + " đến " + to.ToString("dd/MM/yyyy") + "  |  Ngày lập: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "  |  Thu ngân: " + EscXml(cashierName) + @"</t></is></c>
    </row>
    <row r=""4""/>
    <row r=""5"" ht=""20"" customHeight=""1"">
      <c r=""A5"" s=""16"" t=""inlineStr""><is><t>TỔNG DOANH THU</t></is></c>
      <c r=""C5"" s=""16"" t=""inlineStr""><is><t>SỐ ĐƠN HÀNG</t></is></c>
      <c r=""E5"" s=""16"" t=""inlineStr""><is><t>TRUNG BÌNH/ĐƠN</t></is></c>
      <c r=""G5"" s=""16"" t=""inlineStr""><is><t>TỔNG GIẢM GIÁ</t></is></c>
    </row>
    <row r=""6"" ht=""26"" customHeight=""1"">
      <c r=""A6"" s=""17""><v>" + (long)data.Summary.TotalRevenue + @"</v></c>
      <c r=""C6"" s=""17""><v>" + data.Summary.TotalOrders + @"</v></c>
      <c r=""E6"" s=""17""><v>" + (long)data.Summary.AverageOrderValue + @"</v></c>
      <c r=""G6"" s=""17""><v>" + (long)data.Summary.TotalDiscount + @"</v></c>
    </row>
    <row r=""7""/>
    <row r=""8"" ht=""26"" customHeight=""1"">
      <c r=""A8"" s=""4"" t=""inlineStr""><is><t>STT</t></is></c>
      <c r=""B8"" s=""4"" t=""inlineStr""><is><t>Mã đơn hàng</t></is></c>
      <c r=""C8"" s=""4"" t=""inlineStr""><is><t>Thời gian</t></is></c>
      <c r=""D8"" s=""4"" t=""inlineStr""><is><t>Thu ngân</t></is></c>
      <c r=""E8"" s=""4"" t=""inlineStr""><is><t>Khách hàng</t></is></c>
      <c r=""F8"" s=""4"" t=""inlineStr""><is><t>Hình thức TT</t></is></c>
      <c r=""G8"" s=""4"" t=""inlineStr""><is><t>Giảm giá (VNĐ)</t></is></c>
      <c r=""H8"" s=""4"" t=""inlineStr""><is><t>Thành tiền (VNĐ)</t></is></c>
    </row>
");

            int rowIdx = 9;
            int stt = 1;
            decimal sumDiscount = 0;
            decimal sumRevenue = 0;

            foreach (var o in data.Orders)
            {
                bool isZebra = (stt % 2 == 0);
                string customer = string.IsNullOrWhiteSpace(o.CustomerName) ? "Khách vãng lai" : o.CustomerName;
                string method = o.PaymentMethod == "Cash" ? "Tiền mặt" : (o.PaymentMethod == "Card" ? "Thẻ / Chuyển khoản" : (o.PaymentMethod ?? "Khác"));

                sumDiscount += o.DiscountAmount;
                sumRevenue += o.FinalAmount;

                int sText = isZebra ? 9 : 6;
                int sCenter = isZebra ? 10 : 7;
                int sNum = isZebra ? 11 : 8;

                sheetSb.Append($@"    <row r=""{rowIdx}"" ht=""21"" customHeight=""1"">
      <c r=""A{rowIdx}"" s=""{sCenter}""><v>{stt}</v></c>
      <c r=""B{rowIdx}"" s=""{sCenter}"" t=""inlineStr""><is><t>{EscXml(o.OrderCode)}</t></is></c>
      <c r=""C{rowIdx}"" s=""{sCenter}"" t=""inlineStr""><is><t>{o.CreatedAt:dd/MM/yyyy HH:mm}</t></is></c>
      <c r=""D{rowIdx}"" s=""{sText}"" t=""inlineStr""><is><t>{EscXml(o.CashierUsername)}</t></is></c>
      <c r=""E{rowIdx}"" s=""{sText}"" t=""inlineStr""><is><t>{EscXml(customer)}</t></is></c>
      <c r=""F{rowIdx}"" s=""{sCenter}"" t=""inlineStr""><is><t>{EscXml(method)}</t></is></c>
      <c r=""G{rowIdx}"" s=""{sNum}""><v>{(long)o.DiscountAmount}</v></c>
      <c r=""H{rowIdx}"" s=""{sNum}""><v>{(long)o.FinalAmount}</v></c>
    </row>
");
                rowIdx++;
                stt++;
            }

            sheetSb.Append($@"    <row r=""{rowIdx}"" ht=""25"" customHeight=""1"">
      <c r=""A{rowIdx}"" s=""14"" t=""inlineStr""><is><t>TỔNG CỘNG</t></is></c>
      <c r=""B{rowIdx}"" s=""14""/>
      <c r=""C{rowIdx}"" s=""14""/>
      <c r=""D{rowIdx}"" s=""14""/>
      <c r=""E{rowIdx}"" s=""14""/>
      <c r=""F{rowIdx}"" s=""14""/>
      <c r=""G{rowIdx}"" s=""15""><v>{(long)sumDiscount}</v></c>
      <c r=""H{rowIdx}"" s=""15""><v>{(long)sumRevenue}</v></c>
    </row>
  </sheetData>
  <mergeCells count=""8"">
    <mergeCell ref=""A1:H1""/>
    <mergeCell ref=""A2:H2""/>
    <mergeCell ref=""A3:H3""/>
    <mergeCell ref=""A5:B5""/>
    <mergeCell ref=""A6:B6""/>
    <mergeCell ref=""C5:D5""/>
    <mergeCell ref=""C6:D6""/>
    <mergeCell ref=""E5:F5""/>
    <mergeCell ref=""E6:F6""/>
    <mergeCell ref=""G5:H5""/>
    <mergeCell ref=""G6:H6""/>
    <mergeCell ref=""A{rowIdx}:F{rowIdx}""/>
  </mergeCells>
</worksheet>");

            BuildXlsxPackage(filePath, "BaoCaoDoanhThu", sheetSb.ToString());
        }

        #endregion

        #region HÀM ĐÓNG GÓI TỆP TIN OPENXML .XLSX

        private static void BuildXlsxPackage(string filePath, string sheetName, string sheetXml)
        {
            string contentTypes = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
  <Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>
  <Default Extension=""xml"" ContentType=""application/xml""/>
  <Override PartName=""/xl/workbook.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml""/>
  <Override PartName=""/xl/worksheets/sheet1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml""/>
  <Override PartName=""/xl/styles.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml""/>
</Types>";

            string rels = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""xl/workbook.xml""/>
</Relationships>";

            string wbRels = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"" Target=""worksheets/sheet1.xml""/>
  <Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"" Target=""styles.xml""/>
</Relationships>";

            string workbook = $@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<workbook xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"">
  <sheets>
    <sheet name=""{EscXml(sheetName)}"" sheetId=""1"" r:id=""rId1""/>
  </sheets>
</workbook>";

            string styles = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<styleSheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">
  <numFmts count=""1"">
    <numFmt numFmtId=""164"" formatCode=""#,##0""/>
  </numFmts>
  <fonts count=""8"">
    <font><sz val=""10""/><name val=""Segoe UI""/></font>
    <font><b/><sz val=""10""/><color rgb=""FFFFFFFF""/><name val=""Segoe UI""/></font>
    <font><b/><sz val=""15""/><color rgb=""FF1E3A8A""/><name val=""Segoe UI""/></font>
    <font><b/><sz val=""10""/><color rgb=""FF000000""/><name val=""Segoe UI""/></font>
    <font><i/><sz val=""9""/><color rgb=""FF64748B""/><name val=""Segoe UI""/></font>
    <font><b/><sz val=""10""/><color rgb=""FF991B1B""/><name val=""Segoe UI""/></font>
    <font><b/><sz val=""12""/><color rgb=""FF166534""/><name val=""Segoe UI""/></font>
    <font><b/><sz val=""12""/><color rgb=""FF1E40AF""/><name val=""Segoe UI""/></font>
  </fonts>
  <fills count=""8"">
    <fill><patternFill patternType=""none""/></fill>
    <fill><patternFill patternType=""gray125""/></fill>
    <fill><patternFill patternType=""solid""><fgColor rgb=""FF1E40AF""/></patternFill></fill>
    <fill><patternFill patternType=""solid""><fgColor rgb=""FF166534""/></patternFill></fill>
    <fill><patternFill patternType=""solid""><fgColor rgb=""FFF8FAFC""/></patternFill></fill>
    <fill><patternFill patternType=""solid""><fgColor rgb=""FFFEE2E2""/></patternFill></fill>
    <fill><patternFill patternType=""solid""><fgColor rgb=""FFFEF3C7""/></patternFill></fill>
    <fill><patternFill patternType=""solid""><fgColor rgb=""FFE0E7FF""/></patternFill></fill>
  </fills>
  <borders count=""3"">
    <border><left/><right/><top/><bottom/><diagonal/></border>
    <border>
      <left style=""thin""><color rgb=""FFCBD5E1""/></left>
      <right style=""thin""><color rgb=""FFCBD5E1""/></right>
      <top style=""thin""><color rgb=""FFCBD5E1""/></top>
      <bottom style=""thin""><color rgb=""FFCBD5E1""/></bottom>
    </border>
    <border>
      <left style=""thin""><color rgb=""FFCBD5E1""/></left>
      <right style=""thin""><color rgb=""FFCBD5E1""/></right>
      <top style=""thin""><color rgb=""FFCBD5E1""/></top>
      <bottom style=""double""><color rgb=""FF000000""/></bottom>
    </border>
  </borders>
  <cellXfs count=""18"">
    <xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0""/>
    <xf numFmtId=""0"" fontId=""2"" fillId=""0"" borderId=""0"" applyFont=""1""><alignment horizontal=""center""/></xf>
    <xf numFmtId=""0"" fontId=""6"" fillId=""0"" borderId=""0"" applyFont=""1""><alignment horizontal=""center""/></xf>
    <xf numFmtId=""0"" fontId=""4"" fillId=""0"" borderId=""0"" applyFont=""1""/>
    <xf numFmtId=""0"" fontId=""1"" fillId=""2"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""><alignment horizontal=""center"" vertical=""center""/></xf>
    <xf numFmtId=""0"" fontId=""1"" fillId=""3"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""><alignment horizontal=""center"" vertical=""center""/></xf>
    <xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""1"" applyBorder=""1""><alignment horizontal=""left""/></xf>
    <xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""1"" applyBorder=""1""><alignment horizontal=""center""/></xf>
    <xf numFmtId=""164"" fontId=""0"" fillId=""0"" borderId=""1"" applyBorder=""1"" applyNumberFormat=""1""><alignment horizontal=""right""/></xf>
    <xf numFmtId=""0"" fontId=""0"" fillId=""4"" borderId=""1"" applyFill=""1"" applyBorder=""1""><alignment horizontal=""left""/></xf>
    <xf numFmtId=""0"" fontId=""0"" fillId=""4"" borderId=""1"" applyFill=""1"" applyBorder=""1""><alignment horizontal=""center""/></xf>
    <xf numFmtId=""164"" fontId=""0"" fillId=""4"" borderId=""1"" applyFill=""1"" applyBorder=""1"" applyNumberFormat=""1""><alignment horizontal=""right""/></xf>
    <xf numFmtId=""164"" fontId=""5"" fillId=""5"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1"" applyNumberFormat=""1""><alignment horizontal=""right""/></xf>
    <xf numFmtId=""0"" fontId=""5"" fillId=""5"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""><alignment horizontal=""center""/></xf>
    <xf numFmtId=""0"" fontId=""3"" fillId=""6"" borderId=""2"" applyFont=""1"" applyFill=""1"" applyBorder=""1""><alignment horizontal=""right""/></xf>
    <xf numFmtId=""164"" fontId=""3"" fillId=""6"" borderId=""2"" applyFont=""1"" applyFill=""1"" applyBorder=""1"" applyNumberFormat=""1""><alignment horizontal=""right""/></xf>
    <xf numFmtId=""0"" fontId=""3"" fillId=""7"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""><alignment horizontal=""center""/></xf>
    <xf numFmtId=""164"" fontId=""7"" fillId=""7"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1"" applyNumberFormat=""1""><alignment horizontal=""center""/></xf>
  </cellXfs>
</styleSheet>";

            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var zip = new ZipArchive(fs, ZipArchiveMode.Create);

            void WriteEntry(string entryPath, string content)
            {
                var entry = zip.CreateEntry(entryPath, CompressionLevel.Optimal);
                using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
                writer.Write(content);
            }

            WriteEntry("[Content_Types].xml", contentTypes);
            WriteEntry("_rels/.rels", rels);
            WriteEntry("xl/_rels/workbook.xml.rels", wbRels);
            WriteEntry("xl/workbook.xml", workbook);
            WriteEntry("xl/styles.xml", styles);
            WriteEntry("xl/worksheets/sheet1.xml", sheetXml);
        }

        #endregion
    }
}
