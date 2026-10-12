using AIBusinessSupport.Api.Services;
using AIBusinessSupport.Api.Services.VectorStore;
using AIBusinessSupport.API.AI;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AIBusinessSupport.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly DocumentIngestionService _documentIngestionService;
    private readonly PdfTextExtractionService _pdfTextExtractionService;
    private readonly DocumentChunkRepository _documentChunkRepository;  

    public AIController(IAIService aiService, DocumentIngestionService documentIngestionService, 
        PdfTextExtractionService pdfTextExtractionService, DocumentChunkRepository documentChunkRepository)
    {
        _aiService = aiService;
        _documentIngestionService = documentIngestionService;
        _pdfTextExtractionService = pdfTextExtractionService;
        _documentChunkRepository = documentChunkRepository;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("Message is required.");
        }

        var response = await _aiService.ChatAsync(request.ConversationId, request.Message);

        return Ok(new { response });
    }

    [HttpPost("chat/stream")]
    public async Task ChatStream([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            await Response.WriteAsync("Message is required.");
            return;
        }

        Response.ContentType = "text/plain; charset=utf-8";
        Response.Headers.CacheControl = "no-cache";

        await foreach (var chunk in _aiService.ChatStreamAsync(
            request.ConversationId,
            request.Message,
            HttpContext.RequestAborted))
        {
            await Response.WriteAsync(chunk, HttpContext.RequestAborted);
            await Response.Body.FlushAsync(HttpContext.RequestAborted);
        }
    }
    [HttpPost("documents/ingest")]
    public async Task<IActionResult> IngestDocument(
    [FromBody] IngestDocumentRequest request,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.DocumentName) ||
            string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("Document name and text are required.");
        }

        var chunks = await _documentIngestionService.IngestTextAsync(
            request.DocumentName,
            request.Text,
            cancellationToken);

        return Ok(new
        {
            message = "Document ingested successfully.",
            documentName = request.DocumentName,
            chunksCreated = chunks
        });
    }
    [HttpPost("documents/upload-pdf")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPdf(
    IFormFile file,
    CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Please upload a PDF file.");
        }
        const long maxFileSize = 5 * 1024 * 1024; // 5 MB

        if (file.Length > maxFileSize)
        {
            return BadRequest("PDF file size must not exceed 5 MB.");
        }

        if (!string.Equals(
            Path.GetExtension(file.FileName),
            ".pdf",
            StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only PDF files are allowed.");
        }
        await using var stream = file.OpenReadStream();

        var header = new byte[5];
        var bytesRead = await stream.ReadAsync(
            header.AsMemory(0, header.Length),
            cancellationToken);

        if (bytesRead != 5 ||
            !header.AsSpan().SequenceEqual("%PDF-"u8))
        {
            return BadRequest("The uploaded file is not a valid PDF.");
        }

        stream.Position = 0;

        var text = _pdfTextExtractionService.ExtractText(stream);

        var chunks = await _documentIngestionService.IngestTextAsync(
            Path.GetFileName(file.FileName),
            text,
            cancellationToken);

        return Ok(new
        {
            message = "PDF uploaded and ingested successfully.",
            documentName = Path.GetFileName(file.FileName),
            chunksCreated = chunks
        });
    }
    [HttpGet("documents")]
    public async Task<IActionResult> GetDocuments(
    CancellationToken cancellationToken)
    {
        var documents = await _documentChunkRepository.GetDocumentsAsync(
            cancellationToken);

        return Ok(documents);
    }

    [HttpDelete("documents/{documentName}")]
    public async Task<IActionResult> DeleteDocument(
        string documentName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentName))
        {
            return BadRequest("Document name is required.");
        }

        var deletedChunks = await _documentChunkRepository.DeleteDocumentAsync(
            documentName,
            cancellationToken);

        if (deletedChunks == 0)
        {
            return NotFound(new
            {
                message = "Document not found."
            });
        }

        return Ok(new
        {
            message = "Document deleted successfully.",
            documentName,
            deletedChunks
        });
    }

}

public sealed record ChatRequest
{
    public Guid ConversationId { get; set; }
    [Required]
    public required string Message { get; init; }
}
public sealed record IngestDocumentRequest(
    string DocumentName,
    string Text);