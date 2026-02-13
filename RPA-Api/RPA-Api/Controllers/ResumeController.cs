using Microsoft.AspNetCore.Mvc;
using RPA_Api.IService;
using RPA_Api.Model;
using RPA_Api.Repository;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RPA_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResumeController : ControllerBase
    {
        private readonly IResumeStorageService _storage;
        private readonly IResumeRepository _repo;
        private readonly IConfiguration _config;

        public ResumeController(IResumeStorageService storage, IResumeRepository repo, IConfiguration config)
        {
            _storage = storage;
            _repo = repo;
            _config = config;
        }

        [HttpGet]
        public IActionResult Get() => Ok("pong");


        [HttpPost("upload")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> Upload([FromForm]ResumeUploadRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest(new { error = "userId is required." });

            if (request.File == null || request.File.Length == 0)
                return BadRequest(new { error = "No file provided." });

            var allowedTypes = _config.GetSection("Uploads:AllowedContentTypes").Get<string[]>() ?? Array.Empty<string>();
            if (!allowedTypes.Contains(request.File.ContentType, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { error = $"Unsupported content type: {request.File.ContentType}. Allowed: {string.Join(", ", allowedTypes)}" });

            var maxBytes = _config.GetValue<long>("Uploads:MaxFileSizeBytes");
            if (request.File.Length > maxBytes)
                return BadRequest(new { error = $"File too large. Max {maxBytes} bytes." });

            var resumeId = Guid.NewGuid().ToString("N");

            await using var stream = request.File.OpenReadStream();
            var storagePath = await _storage.UploadResumeAsync(
                userId: request.UserId,
                fileName: request.File.FileName,
                contentType: request.File.ContentType,
                content: stream,
                ct: request.CT);

            var record = new ResumeRecord
            {
                ResumeId = resumeId,
                UserId = request.UserId,
                FileName = request.File.FileName,
                ContentType = request.File.ContentType,
                SizeBytes = request.File.Length,
                StoragePath = storagePath,
                UploadedAt = DateTimeOffset.UtcNow,
                Status = "Uploaded"
            };

            await _repo.CreateAsync(record, request.CT);

            return Ok(new
            {
                resumeId,
                storagePath,
                status = record.Status
            });
        }
    }
}
