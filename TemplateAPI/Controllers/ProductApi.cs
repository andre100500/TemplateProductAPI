using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using TemplateAPI.Data;
using TemplateAPI.Models;

namespace TemplateAPI.Controllers
{
    public class ProductApi : ControllerBase
    {
        private readonly ItemDBContext _context;
        private readonly string rootPath;

        public ProductApi(ItemDBContext context)
        {
            _context = context;
            rootPath = AppDomain.CurrentDomain.BaseDirectory;
        }

        [HttpGet("product/{id}")]
        public async Task<ActionResult<TodoItem>> Get(int id)
        {
            var images = await _context.Item.FindAsync(id);

            if (images == null)
            {
                return NotFound();
            }
            return File(images.Data, "image/jpeg", images.Name);
        }

        [HttpGet("product/images/{id}")]
        public async Task<ActionResult<TodoItem>> GetImages(int[] id)
        {
            var images = await _context.Item.Where(i => id.Contains(i.Id)).ToListAsync();

            if (images.Count == 0)
            {
                return NotFound();
            }

            var archiveName = $"images_{DateTime.UtcNow:yyyyMMddHHmmss}.zip";
            using var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                foreach(var image in images)
                {
                    var entry = archive.CreateEntry(image.Name);
                    using var entryStream = entry.Open();
                    await entryStream.WriteAsync(image.Data);
                }
            }
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, "product/zip", archiveName);
        }
        //TODO testing & fixis app 
        [HttpGet("product/images/upload/{path}")]
        public  ActionResult FileUploadedImage(string fileName)
        {
            string filePath = Path.Combine(rootPath, fileName);
            try
            {
                var fileStream = new FileStream(filePath,FileMode.Open,FileAccess.Read);

                fileStream.Close();

                return File(fileStream, "image/jpeg");
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex);
            }
        }

        [HttpPost("product/images/upload")]
        public async Task<ActionResult<TodoItem>> UploadeImage()
        {
            var files = Request.Form.Files;
            if(files == null || files.Count == 0)
            {
                return BadRequest("No files were selected");
            }
            foreach(var file in files)
            {
                var image = new TodoItem { Name = file.Name };
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    image.Data = stream.ToArray();
                }
                _context.Item.Add(image);
            }
            await _context.SaveChangesAsync();
            return Ok("Images uploaded successfully.");
            
        }
    }
}
