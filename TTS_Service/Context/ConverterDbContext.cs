using Microsoft.EntityFrameworkCore;
using TTS_Service.Models;

namespace TTS_Service.Context;

public class ConverterDbContext : DbContext
{
    public DbSet<TssModel> TssModel { get; set; }

    public ConverterDbContext(DbContextOptions<ConverterDbContext> options)
    : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TssModel>(builder =>
        {
            builder.HasKey(m => m.Id);
        });
    }
}
