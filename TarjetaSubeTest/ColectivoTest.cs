namespace TarjetaSubeTest;

using NUnit.Framework;
using TarjetaSube;

[TestFixture]
public class ColectivoTest
{
    [Test]
    public void PagarCon_ConSaldoSuficiente_ViajeExitosoYDescuentaSaldo()
    {
        var colectivo = new Colectivo("102 Roja");
        var tarjeta = new Tarjeta(5000m);

        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(3420m));
    }

    [Test]
    public void PagarCon_ConSaldoExacto_ViajeExitosoYSaldoQuedaEnCero()
    {
        var colectivo = new Colectivo("144 Negra");
        var tarjeta = new Tarjeta(1580m);

        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(0m));
    }

    [Test]
    public void PagarCon_ViajeExitoso_EmiteBoletoConDatosCorrectos()
    {
        var colectivo = new Colectivo("122 Verde");
        var tarjeta = new Tarjeta(4000m) { Id = 42 };
        var fechaAntes = DateTime.Now;

        var boleto = colectivo.pagarCon(tarjeta);
        var fechaDespues = DateTime.Now;

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Linea, Is.EqualTo("122 Verde"));
        Assert.That(boleto.MontoPagado, Is.EqualTo(1580m));
        Assert.That(boleto.SaldoRestante, Is.EqualTo(2420m));
        Assert.That(boleto.TarjetaId, Is.EqualTo(42));
        Assert.That(boleto.Fecha, Is.GreaterThanOrEqualTo(fechaAntes));
        Assert.That(boleto.Fecha, Is.LessThanOrEqualTo(fechaDespues));
    }

    [Test]
    public void PagarCon_ConSaldoInsuficiente_RetornaNullYNoModificaSaldo()
    {
        var colectivo = new Colectivo("102 Roja");
        var tarjeta = new Tarjeta();
        colectivo.pagarCon(tarjeta); // 1er viaje plus, saldo queda en -1580m

        // El segundo viaje requeriría -1580m - 1580m = -3160m < LimiteNegativo (-2000m)
        var segundoBoleto = colectivo.pagarCon(tarjeta);

        Assert.That(segundoBoleto, Is.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-1580m));
    }

    [Test]
    public void PagarCon_ConSaldoCero_PermiteViajePlusYSaldoQuedaNegativo()
    {
        var colectivo = new Colectivo("K");
        var tarjeta = new Tarjeta();

        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-1580m));
    }

    [Test]
    public void PagarCon_ViajesConsecutivos_TercerViajeFallaPorSuperarLimiteNegativo()
    {
        var colectivo = new Colectivo("115");
        var tarjeta = new Tarjeta(2000m);

        var primerBoleto = colectivo.pagarCon(tarjeta);
        Assert.That(primerBoleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(420m));

        var segundoBoleto = colectivo.pagarCon(tarjeta);
        Assert.That(segundoBoleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-1160m));

        var tercerBoleto = colectivo.pagarCon(tarjeta);
        Assert.That(tercerBoleto, Is.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-1160m));
    }

    [Test]
    public void PagarCon_MultiplesViajesConsecutivosConSaldoSuficiente_DescuentaSaldoAcumulado()
    {
        var colectivo = new Colectivo("102 Roja");
        var tarjeta = new Tarjeta(5000m);

        var primerBoleto = colectivo.PagarCon(tarjeta);
        Assert.That(primerBoleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(3420m));
        Assert.That(primerBoleto!.SaldoRestante, Is.EqualTo(3420m));

        var segundoBoleto = colectivo.PagarCon(tarjeta);
        Assert.That(segundoBoleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(1840m));
        Assert.That(segundoBoleto!.SaldoRestante, Is.EqualTo(1840m));
    }

    [Test]
    public void PagarCon_TarjetaNull_RetornaNull()
    {
        var colectivo = new Colectivo("102");

        var boleto = colectivo.pagarCon(null!);

        Assert.That(boleto, Is.Null);
    }

    [Test]
    public void Constructor_AsignaLineaCorrectamente()
    {
        var colectivo = new Colectivo("Línea de la Costa");

        Assert.That(colectivo.Linea, Is.EqualTo("Línea de la Costa"));
        Assert.That(colectivo.Tarifa, Is.EqualTo(1580m));
        Assert.That(Colectivo.TarifaBasica, Is.EqualTo(1580m));
    }
}
