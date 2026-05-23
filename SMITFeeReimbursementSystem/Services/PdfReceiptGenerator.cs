using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SMITFeeReimbursementSystem.Models;

namespace SMITFeeReimbursementSystem.Services;

public static class PdfReceiptGenerator
{
    static PdfReceiptGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static byte[] Generate(Receipt receipt)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text("SMIT Fee Reimbursement System")
                        .Bold().FontSize(20).FontColor(Colors.Blue.Darken2);
                    col.Item().Text("Official Payment Receipt").FontSize(12).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"Receipt No: {receipt.ReceiptNumber}").Bold();
                        row.RelativeItem().AlignRight().Text($"Date: {receipt.GeneratedAt:dd MMM yyyy HH:mm}");
                    });

                    col.Item().PaddingTop(16).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(140);
                            columns.RelativeColumn();
                        });

                        AddRow(table, "Student Name", receipt.StudentName);
                        AddRow(table, "Roll Number", string.IsNullOrWhiteSpace(receipt.RollNumber) ? "—" : receipt.RollNumber);
                        AddRow(table, "Course", receipt.CourseName);
                        AddRow(table, "Amount Paid", $"Rs. {receipt.Amount:N0}");
                        AddRow(table, "Transaction ID", receipt.TransactionId);
                        AddRow(table, "Payment Date", receipt.PaymentDate.ToString("dd MMMM yyyy"));
                        AddRow(table, "Approved By", receipt.ApprovedByName);
                    });

                    col.Item().PaddingTop(24).Background(Colors.Green.Lighten4).Padding(12)
                        .Text("Payment verified and approved. This is an official system-generated receipt.")
                        .FontSize(10).Italic();
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("SMIT Institute · ").FontSize(9);
                    text.Span("Fee Reimbursement System").FontSize(9).Italic();
                });
            });
        }).GeneratePdf();
    }

    private static void AddRow(TableDescriptor table, string label, string value)
    {
        table.Cell().PaddingVertical(6).Text(label).SemiBold();
        table.Cell().PaddingVertical(6).Text(value);
    }
}
