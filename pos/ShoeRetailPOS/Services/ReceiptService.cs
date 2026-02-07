using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using ShoeRetailPOS.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ShoeRetailPOS.Services
{
    public static class ReceiptService
    {
        public static void GenerateReceipt(IEnumerable<Product> items,decimal total)
        {
            string folderPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Receipts");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(
                folderPath,
                $"Receipt_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

            // 🧾 80mm Thermal Width
            float receiptWidth = 226f; // ~80mm
            float receiptHeight = 1000f; // dynamic enough

            var pageSize = new iText.Kernel.Geom.PageSize(
                receiptWidth,
                receiptHeight);

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf, pageSize);

            document.SetMargins(10, 10, 10, 10);

            PdfFont normal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            PdfFont bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            // 🏪 STORE NAME
            document.Add(new Paragraph("SHOE MART")
                .SetFont(bold)
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("SALES RECEIPT")
                .SetFont(normal)
                .SetFontSize(9)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph($"Date: {DateTime.Now:yyyy-MM-dd HH:mm}")
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("--------------------------------"));

            // 🛒 ITEMS
            foreach (var item in items)
            {
                document.Add(new Paragraph(
                    $"{item.Name} ({item.SelectedSize})")
                    .SetFont(bold)
                    .SetFontSize(8));

                document.Add(new Paragraph(
                    $" {item.Quantity} x {item.Price:N2}  =  {(item.Price * item.Quantity):N2}")
                    .SetFont(normal)
                    .SetFontSize(8));
            }

            document.Add(new Paragraph("--------------------------------"));

            // 💰 TOTAL
            document.Add(new Paragraph($"TOTAL: Rs. {total:N2}")
                .SetFont(bold)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.RIGHT));

            document.Add(new Paragraph("\nThank you!")
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Close();

            PrintReceipt(filePath);

        }

        public static void PrintReceipt(string filePath)
        {
            var psi = new ProcessStartInfo
            {
                FileName = filePath,
                Verb = "print",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true
            };

            Process.Start(psi);
        }

    }
}
