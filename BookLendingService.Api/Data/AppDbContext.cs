using BookLendingService.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace BookLendingService.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books => Set<Book>();
    }
}