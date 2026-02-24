using Microsoft.AspNetCore.Mvc;
using RPA_Api.IService;
using RPA_Api.Model;
using RPA_Api.Repository;
using static Google.Rpc.Context.AttributeContext.Types;

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
        public async Task<ActionResult<ResumeCollection>> GetResumeByUserId(String userId)
        {
            if (string.IsNullOrWhiteSpace(userId) || userId.Equals("string"))
            {
                return BadRequest(new {error="NoUid"});
            }

           var response = await _storage.GetResumesByUserId(userId);

            return Ok(response);
        }


        [HttpPost("upload")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> Upload([FromForm]ResumeUploadRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId)||request.UserId.Equals("string"))
            {
                request.UserId = Guid.NewGuid().ToString();
            }
                

            if (request.File == null || request.File.Length == 0)
                return BadRequest(new { error = "No file provided." });



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

            //await _repo.CreateAsync(record, request.CT);

            return Ok(new
            {
                UserId = record.UserId,
                resumeId,
                storagePath,
                status = record.Status
            });
        }




    }
}
