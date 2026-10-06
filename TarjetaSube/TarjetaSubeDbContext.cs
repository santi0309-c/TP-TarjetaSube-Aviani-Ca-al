namespace TarjetaSube;

using Microsoft.EntityFrameworkCore;

public class TarjetaSubeDbContext : DbContext
{
    public TarjetaSubeDbContext()
    {
    }

    public TarjetaSubeDbContext(DbContextOptions<TarjetaSubeDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tarjeta> Tarjetas { get; set; } = null!;
    public DbSet<Colectivo> Colectivos { get; set; } = null!;
    public DbSet<Boleto> Boletos { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseInMemoryDatabase("TarjetaSubeDb");
        }
    }
}
