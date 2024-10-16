using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using catalog.Models;

namespace catalog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly string _mediaFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Media");

        public MediaController()
        {
            if (!Directory.Exists(_mediaFolder))
            {
                Directory.CreateDirectory(_mediaFolder);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadFiles(IList<IFormFile> files)
        {
            try
            {
                foreach (var file in files)
                {
                    if (file.Length > 0 && file.ContentType == "video/mp4")
                    {
                        if (file.Length > 209715200) //200* 1024 * 1024 200MB limit
                        {
                            return BadRequest("File size exceeds 200MB limit.");
                        }

                        var filePath = Path.Combine(_mediaFolder, file.FileName);
                        //Copies the file content to the newly created file stream asynchronously.
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        // GET: api/media
        [HttpGet]
        public IActionResult GetVideoFiles()
        {
            var files = Directory.GetFiles(_mediaFolder)
                                 .Select(file => new VideoFile
                                 {
                                     FileName = Path.GetFileName(file),
                                     FileSize = new FileInfo(file).Length
                                 })
                                 .ToList();
            return Ok(files);
        }
    }
}