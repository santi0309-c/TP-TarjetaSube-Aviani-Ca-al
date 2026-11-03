namespace TarjetaSubeTest;

using NUnit.Framework;
using TarjetaSube;

[TestFixture]
public class InterfacesTest
{
    private static readonly DateTime FechaHabil = new(2026, 10, 19, 10, 0, 0);

    [Test]
    public void ITransporte_PermiteOperarColectivoUrbanoEInterurbano()
    {
        ITransporte urbano = new Colectivo("102 Roja");
        ITransporte interurbano = new ColectivoInterurbano("Expreso");
        var tarjeta = new Tarjeta(10000m);

        Assert.That(urbano.Linea, Is.EqualTo("102 Roja"));
        Assert.That(urbano.Tarifa, Is.EqualTo(Colectivo.TarifaBasica));

        Assert.That(interurbano.Linea, Is.EqualTo("Expreso"));
        Assert.That(interurbano.Tarifa, Is.EqualTo(ColectivoInterurbano.TarifaInterurbanaFija));

        var boleto1 = urbano.PagarCon(tarjeta, FechaHabil);
        Assert.That(boleto1, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(10000m - 2150m));

        var boleto2 = interurbano.PagarCon(tarjeta, FechaHabil.AddHours(2));
        Assert.That(boleto2, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(10000m - 2150m - 3000m));
    }

    [Test]
    public void IServicioBici_PermiteOperarEstacionBiciYMiBiciTuBici()
    {
        IServicioBici estacion = new EstacionBici("Estación Central");
        IServicioBici miBici = new MiBiciTuBici("Estación Pellegrini");
        var tarjeta = new Tarjeta(10000m);

        Assert.That(estacion.Nombre, Is.EqualTo("Estación Central"));
        Assert.That(estacion.EstaEnUso, Is.False);

        Assert.That(miBici.Nombre, Is.EqualTo("Estación Pellegrini"));
        Assert.That(miBici.EstaEnUso, Is.False);

        bool retiro = estacion.RetirarBicicleta(tarjeta, FechaHabil);
        Assert.That(retiro, Is.True);
        Assert.That(estacion.EstaEnUso, Is.True);

        bool devolucion = estacion.DevolverBicicleta(tarjeta, FechaHabil.AddMinutes(30));
        Assert.That(devolucion, Is.True);
        Assert.That(estacion.EstaEnUso, Is.False);
    }

    [Test]
    public void ICobrable_DescuentaSaldoSinConocerImplementacionConcreta()
    {
        ICobrable cobrable = new Tarjeta(5000m);

        Assert.That(cobrable.Saldo, Is.EqualTo(5000m));

        bool resultado = cobrable.DescontarSaldo(2000m);

        Assert.That(resultado, Is.True);
        Assert.That(cobrable.Saldo, Is.EqualTo(3000m));
    }
}

