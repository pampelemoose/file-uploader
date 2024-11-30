using Microsoft.EntityFrameworkCore;

namespace api_server.Models;

public class UploadDbContext : DbContext
{
    public UploadDbContext(DbContextOptions<UploadDbContext> options)
    : base(options)
    {
    }

    public DbSet<Transaction> Transactions { get; set; }
}
