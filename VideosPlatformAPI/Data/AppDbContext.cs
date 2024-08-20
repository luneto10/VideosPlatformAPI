using Microsoft.EntityFrameworkCore;
using VideosPlatformAPI.Models;

namespace VideosPlatformAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }

    public DbSet<Video> Videos { get; set; }
    public DbSet<Category> Categories { get; set; }
}