namespace TarjetaSubeTest;

using NUnit.Framework;
using TarjetaSube;

[TestFixture]
public class TimeProviderTest
{
    private static readonly DateTime FechaLunesBase = new(2026, 10, 19, 10, 0, 0);

    [Test]
    public void SystemTimeProvider_RetornaFechaYHoraActual()
    {
        var provider = new SystemTimeProvider();
        var antes = DateTime.Now;
        var now = provider.Now;
        var despues = DateTime.Now;

        Assert.That(now, Is.GreaterThanOrEqualTo(antes));
        Assert.That(now, Is.LessThanOrEqualTo(despues));
    }

    [Test]
    public void Colectivo_ConFakeTimeProviderInyectado_PermiteTrasbordoSinEspecificarFechasManuales()
    {
        var fakeTime = new FakeTimeProvider(FechaLunesBase);
        var colectivo1 = new Colectivo("102", esInterurbano: false, fakeTime);
        var colectivo2 = new Colectivo("144", esInterurbano: false, fakeTime);
        var tarjeta = new Tarjeta(10000m);

        // Primer viaje a las 10:00 (hora simulada)
        var boleto1 = colectivo1.PagarCon(tarjeta);
        Assert.That(boleto1, Is.Not.Null);
        Assert.That(boleto1!.Fecha, Is.EqualTo(FechaLunesBase));
        Assert.That(tarjeta.Saldo, Is.EqualTo(10000m - 2150m));

        // Avanzamos 25 minutos en el proveedor simulado
        fakeTime.AvanzarMinutos(25);

        // Segundo viaje en colectivo distinto dentro de los 60 min -> trasbordo automático
        var boleto2 = colectivo2.PagarCon(tarjeta);
        Assert.That(boleto2, Is.Not.Null);
        Assert.That(boleto2!.EsTrasbordo, Is.True);
        Assert.That(boleto2.MontoPagado, Is.EqualTo(0m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(10000m - 2150m));
    }

    [Test]
    public void Colectivo_ConFakeTimeProviderInyectado_RechazaFranquiciaEnFinDeSemana()
    {
        var domingo = new DateTime(2026, 10, 25, 12, 0, 0);
        var fakeTime = new FakeTimeProvider(domingo);
        var colectivo = new Colectivo("102", esInterurbano: false, fakeTime);
        var tarjeta = new Tarjeta(5000m, TarjetaTipo.MedioBoleto);

        var boleto = colectivo.PagarCon(tarjeta);

        Assert.That(boleto, Is.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m));
    }

    [Test]
    public void EstacionBici_ConFakeTimeProviderInyectado_GestionaDevolucionYMultasDeterministas()
    {
        var fakeTime = new FakeTimeProvider(FechaLunesBase);
        var estacion = new EstacionBici("Estación Central", fakeTime);
        var tarjeta = new Tarjeta(10000m);

        // Retiro a las 10:00 simuladas
        bool retiro = estacion.RetirarBicicleta(tarjeta);
        Assert.That(retiro, Is.True);
        Assert.That(estacion.EstaEnUso, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(10000m - EstacionBici.TarifaDiaria));

        // Avanzamos 75 minutos (excede la hora de uso)
        fakeTime.AvanzarMinutos(75);

        // Devolución: genera multa por exceso de tiempo
        bool devolucion = estacion.DevolverBicicleta(tarjeta);
        Assert.That(devolucion, Is.True);
        Assert.That(estacion.EstaEnUso, Is.False);
        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(EstacionBici.MultaExceso));

        // Avanzamos al día siguiente
        fakeTime.AvanzarDias(1);

        // Retiro al día siguiente: cobra tarifa diaria nueva + multa pendiente
        decimal saldoAntes = tarjeta.Saldo;
        bool segundoRetiro = estacion.RetirarBicicleta(tarjeta);
        Assert.That(segundoRetiro, Is.True);
        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(0m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(saldoAntes - EstacionBici.TarifaDiaria - EstacionBici.MultaExceso));
    }
}
