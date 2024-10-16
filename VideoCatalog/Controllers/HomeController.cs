using catalog.Models;
using Microsoft.AspNetCore.Mvc;



namespace catalog.Controllers
{
    public class HomeController : Controller
    {
        // created By MaaniG
        private readonly string _mediaPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Media");
        // HttpClient instance provided via Dependency Injection
        private readonly HttpClient _httpClient;

        public HomeController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var files = await GetVideoFilesFromApi();
            return View(files);
        }
        // Handles POST requests to upload files
        [HttpPost]
        public async Task<IActionResult> UploadFiles(IList<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                ModelState.AddModelError("", "No files selected.");
                return View("Index", await GetVideoFilesFromApi());
            }

            var content = new MultipartFormDataContent();
            foreach (var file in files)
            {
                if (file.Length > 0 && file.FileName.EndsWith(".mp4"))
                {
                    // Check if the file is a valid MP4 file
                    var streamContent = new StreamContent(file.OpenReadStream());
                    // adding the type 
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                    content.Add(streamContent, "files", file.FileName);
                }
                else
                {
                    ModelState.AddModelError("", "Only MP4 files are allowed.");
                    return View("Index", await GetVideoFilesFromApi());
                }
            }

            //var response = await _httpClient.PostAsync("https://localhost:7269/api/upload", content); changing the port

            var uploadUrl = Url.Action("UploadFiles", "Media", null, Request.Scheme);
            // Send an HTTP POST request to upload api with the file content
            var response = await _httpClient.PostAsync(uploadUrl, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Error uploading files.");
                return View("Index", await GetVideoFilesFromApi());
            }
        }
        // Retrieves the list of video files from the media api 
        private async Task<List<VideoFile>> GetVideoFilesFromApi()
        {
            var getUrl = Url.Action("GetVideoFiles", "Media", null, Request.Scheme);
            var response = await _httpClient.GetFromJsonAsync<List<VideoFile>>(getUrl);
            return response ?? new List<VideoFile>();
        }
    }
}
