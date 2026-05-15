using CRUDEFMigrations.Models;
using Microsoft.EntityFrameworkCore;

namespace CRUDEFMigrations.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
    }
}
