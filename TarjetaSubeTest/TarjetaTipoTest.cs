namespace TarjetaSubeTest;

using NUnit.Framework;
using TarjetaSube;

[TestFixture]
public class TarjetaTipoTest
{
    [Test]
    public void FranquiciaCompleta_SiemprePuedePagar_InclusoConSaldoCero()
    {
        var tipoFranquiciaCompleta = new TarjetaTipo("Franquicia Completa", 0m);
        var tarjeta = new Tarjeta(0m, tipoFranquiciaCompleta) { Id = 10 };
        var colectivo = new Colectivo("144 Negra");

        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.MontoPagado, Is.EqualTo(0m));
        Assert.That(boleto.SaldoRestante, Is.EqualTo(0m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(0m));
    }

    [Test]
    public void MedioBoleto_PagaExactamenteLaMitadDeTarifaNormal()
    {
        var tipoMedioBoleto = new TarjetaTipo("Medio Boleto Estudiantil", 0.5m);
        var tarjeta = new Tarjeta(2000m, tipoMedioBoleto) { Id = 20 };
        var colectivo = new Colectivo("102 Roja");

        decimal tarifaEsperada = Colectivo.TarifaBasica / 2m; // 790m
        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.MontoPagado, Is.EqualTo(tarifaEsperada));
        Assert.That(boleto.SaldoRestante, Is.EqualTo(2000m - tarifaEsperada));
        Assert.That(tarjeta.Saldo, Is.EqualTo(2000m - tarifaEsperada));
    }

    [Test]
    public void BoletoGratuitoEstudiantil_PagaCeroYEmiteBoletoCorrecto()
    {
        var tipoGratuito = new TarjetaTipo("Boleto Gratuito Estudiantil", 0m);
        var tarjeta = new Tarjeta(100m, tipoGratuito);
        var colectivo = new Colectivo("K");

        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.MontoPagado, Is.EqualTo(0m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(100m));
    }

    [Test]
    public void TarjetaNormal_PagaTarifaCompleta()
    {
        var tipoNormal = new TarjetaTipo("Normal", 1m);
        var tarjeta = new Tarjeta(3000m, tipoNormal);
        var colectivo = new Colectivo("115");

        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.MontoPagado, Is.EqualTo(Colectivo.TarifaBasica));
        Assert.That(tarjeta.Saldo, Is.EqualTo(3000m - Colectivo.TarifaBasica));
    }

    [Test]
    public void AgregarNuevoTipo_SinCrearNuevaClase_CalculaPrecioYPermitePagar()
    {
        // Se puede crear cualquier tipo nuevo dinámicamente sin subclases
        var tipoTarifaSocial = new TarjetaTipo("Tarifa Social", 0.45m);
        var tarjeta = new Tarjeta(2000m, tipoTarifaSocial);
        var colectivo = new Colectivo("122 Verde");

        decimal tarifaEsperada = Colectivo.TarifaBasica * 0.45m;
        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.MontoPagado, Is.EqualTo(tarifaEsperada));
        Assert.That(tarjeta.Saldo, Is.EqualTo(2000m - tarifaEsperada));
    }

    [Test]
    public void CalcularPrecio_SinTipoAsignado_DevuelveTarifaBase()
    {
        var tarjeta = new Tarjeta(2000m);

        decimal precio = tarjeta.CalcularPrecio(Colectivo.TarifaBasica);

        Assert.That(precio, Is.EqualTo(Colectivo.TarifaBasica));
    }
}
