using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using TemplateAPI.Data;
using TemplateAPI.Models;

namespace TemplateAPI.Controllers
{
    public class ProductApi : ControllerBase
    {
        private readonly ItemDBContext _context;

        public ProductApi(ItemDBContext context)
        {
            _context = context;
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
        [HttpPost("product/{todoItem}")]
        public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem todoItem)
        {
            _context.Item.Add(todoItem);
            await _context.SaveChangesAsync();

            //    return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
            return CreatedAtAction(nameof(todoItem), new { id = todoItem.Id }, todoItem);
        }
    }
}
