using Microsoft.EntityFrameworkCore;
using TemplateAPI.Models;

namespace TemplateAPI.Data
{
    public class ItemDBContext : DbContext
    {
        public ItemDBContext(DbContextOptions<ItemDBContext> options) : base(options)
        {

        }

        public DbSet<TodoItem> Item { get; set; }
    }
}
