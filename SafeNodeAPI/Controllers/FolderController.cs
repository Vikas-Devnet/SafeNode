using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeNodeAPI.Models.Request;
using SafeNodeAPI.Src.Services.DocumentFolder;
using System.Security.Claims;

namespace SafeNodeAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FolderController(IFolderService _folderService) : ControllerBase
    {
        [HttpPost("createFolder")]
        public async Task<IActionResult> CreateFolder([FromBody] FolderRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var folder = await _folderService.CreateFolderAsync(request, userId);
                return Ok(folder);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }

        }
    }
}
