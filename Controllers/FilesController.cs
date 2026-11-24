
using Microsoft.AspNetCore.Mvc;

namespace SarabRestaurant.Controllers
{
    public class FilesController : Controller
    {
        private readonly string _filesPath;

        private static readonly string[] AllowedExtensions =
        {
            ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".txt"
        };

        public FilesController(IWebHostEnvironment environment)
        {
            _filesPath = Path.Combine(
                environment.WebRootPath, "Files");

            Directory.CreateDirectory(_filesPath);
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Error = "Please select a file.";
                return View();
            }

            var extension = Path.GetExtension(
                file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                ViewBag.Error = "Unsupported file type.";
                return View();
            }

            if (file.Length > 10 * 1024 * 1024)
            {
                ViewBag.Error = "Maximum file size is 10 MB.";
                return View();
            }

            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(_filesPath, fileName);

            if (System.IO.File.Exists(filePath))
            {
                ViewBag.Error = "This file already exists.";
                return View();
            }

            using (var stream = new FileStream(
                filePath, FileMode.CreateNew))
            {
                await file.CopyToAsync(stream);
            }

            TempData["Success"] = "File uploaded successfully!";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Index()
        {
            var files = Directory.GetFiles(_filesPath)
                .Select(path => Path.GetFileName(path))
                .ToList();

            return View(files);
        }
    }
}
