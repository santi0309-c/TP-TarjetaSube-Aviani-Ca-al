namespace TarjetaSubeTest;

using NUnit.Framework;
using TarjetaSube;

[TestFixture]
public class BoletoTest
{
    [Test]
    public void Constructor_ConParametros_InicializaPropiedadesCorrectamente()
    {
        var fecha = new DateTime(2026, 9, 19, 10, 30, 0);
        string linea = "102 Roja";
        decimal montoPagado = 1580m;
        decimal saldoRestante = 3420m;
        int tarjetaId = 1;

        var boleto = new Boleto(linea, montoPagado, saldoRestante, tarjetaId, fecha);

        Assert.That(boleto.Linea, Is.EqualTo("102 Roja"));
        Assert.That(boleto.MontoPagado, Is.EqualTo(1580m));
        Assert.That(boleto.SaldoRestante, Is.EqualTo(3420m));
        Assert.That(boleto.TarjetaId, Is.EqualTo(1));
        Assert.That(boleto.Fecha, Is.EqualTo(fecha));
    }

    [Test]
    public void Constructor_SinFecha_AsignaFechaActual()
    {
        var fechaAntes = DateTime.Now;

        var boleto = new Boleto("144 Negra", 1580m, 5000m, 2);

        var fechaDespues = DateTime.Now;

        Assert.That(boleto.Fecha, Is.GreaterThanOrEqualTo(fechaAntes));
        Assert.That(boleto.Fecha, Is.LessThanOrEqualTo(fechaDespues));
    }
}
