using UglyToad.PdfPig;
using System.Text;

namespace AIBusinessSupport.Api.Services;

public sealed class PdfTextExtractionService
{
    public string ExtractText(Stream pdfStream)
    {
        using var document = PdfDocument.Open(pdfStream);
        var text = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            text.AppendLine(page.Text);
        }

        var extractedText = text.ToString().Trim();

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            throw new InvalidOperationException(
                "No readable text was found in the PDF.");
        }

        return extractedText;
    }
}