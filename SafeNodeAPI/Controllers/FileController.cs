using Microsoft.AspNetCore.Mvc;
using SafeNodeAPI.Models.Request;
using SafeNodeAPI.Src.Services.UserFiles;
using System.Security.Claims;

namespace SafeNodeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController(IFileService _fileService) : ControllerBase
    {
        [HttpPost("uploadFile")]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var response = await _fileService.UploadFileAsync(request, userId);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [HttpGet("downloadFile/{fileId}")]
        public async Task<IActionResult> DownloadFile(int fileId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var (stream, contentType, fileName) = await _fileService.DownloadFileAsync(fileId, userId);
                return File(stream, contentType, fileName);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [HttpDelete("deleteFile/{fileId}")]
        public async Task<IActionResult> DeleteFile(int fileId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var response = await _fileService.DeleteFileAsync(fileId, userId);
                if (response == null)
                {
                    return NotFound(new { message = "File not found or you do not have permission to delete it." });
                }
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [HttpGet("getFilesByFolderId")]
        public async Task<IActionResult> GetFilesByFolderId([FromQuery] int? folderId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var files = await _fileService.GetFilesByFolderIdAsync(folderId, userId);
                return Ok(files);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
