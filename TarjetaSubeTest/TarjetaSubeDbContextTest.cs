namespace TarjetaSubeTest;

using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TarjetaSube;

[TestFixture]
public class TarjetaSubeDbContextTest
{
    private DbContextOptions<TarjetaSubeDbContext> _options = null!;

    [SetUp]
    public void Setup()
    {
        _options = new DbContextOptionsBuilder<TarjetaSubeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Test]
    public void DbContext_PermiteGuardarYConsultarTarjetas()
    {
        using (var context = new TarjetaSubeDbContext(_options))
        {
            var tarjeta = new Tarjeta(5000m);
            context.Tarjetas.Add(tarjeta);
            context.SaveChanges();
        }

        using (var context = new TarjetaSubeDbContext(_options))
        {
            var tarjeta = context.Tarjetas.FirstOrDefault();
            Assert.That(tarjeta, Is.Not.Null);
            Assert.That(tarjeta!.Saldo, Is.EqualTo(5000m));
        }
    }

    [Test]
    public void DbContext_PermiteGuardarYConsultarColectivos()
    {
        using (var context = new TarjetaSubeDbContext(_options))
        {
            var colectivo = new Colectivo("102 Roja");
            context.Colectivos.Add(colectivo);
            context.SaveChanges();
        }

        using (var context = new TarjetaSubeDbContext(_options))
        {
            var colectivo = context.Colectivos.FirstOrDefault();
            Assert.That(colectivo, Is.Not.Null);
            Assert.That(colectivo!.Linea, Is.EqualTo("102 Roja"));
        }
    }

    [Test]
    public void DbContext_PermiteGuardarYConsultarBoletos()
    {
        var fecha = new DateTime(2026, 9, 26, 12, 0, 0);

        using (var context = new TarjetaSubeDbContext(_options))
        {
            var boleto = new Boleto("144 Negra", 1580m, 3420m, 1, fecha);
            context.Boletos.Add(boleto);
            context.SaveChanges();
        }

        using (var context = new TarjetaSubeDbContext(_options))
        {
            var boleto = context.Boletos.FirstOrDefault();
            Assert.That(boleto, Is.Not.Null);
            Assert.That(boleto!.Linea, Is.EqualTo("144 Negra"));
            Assert.That(boleto.MontoPagado, Is.EqualTo(1580m));
            Assert.That(boleto.SaldoRestante, Is.EqualTo(3420m));
            Assert.That(boleto.TarjetaId, Is.EqualTo(1));
            Assert.That(boleto.Fecha, Is.EqualTo(fecha));
        }
    }

    [Test]
    public void DbContext_ConstructorSinParametros_SeInstanciaCorrectamente()
    {
        using var context = new TarjetaSubeDbContext();
        Assert.That(context.Tarjetas, Is.Not.Null);
        Assert.That(context.Colectivos, Is.Not.Null);
        Assert.That(context.Boletos, Is.Not.Null);
    }
}
