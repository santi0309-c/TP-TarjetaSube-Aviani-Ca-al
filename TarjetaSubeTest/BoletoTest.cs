namespace TarjetaSubeTest;

using NUnit.Framework;
using TarjetaSube;

[TestFixture]
public class BoletoTest
{
    private static readonly DateTime FechaBaseLunes = new(2026, 10, 19, 10, 0, 0);

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

    [Test]
    public void Constructor_PorDefecto_InicializaValoresPredeterminados()
    {
        var boleto = new Boleto();

        Assert.That(boleto.Linea, Is.Empty);
        Assert.That(boleto.MontoPagado, Is.EqualTo(0m));
        Assert.That(boleto.SaldoRestante, Is.EqualTo(0m));
        Assert.That(boleto.TarjetaId, Is.EqualTo(0));
        Assert.That(boleto.EsTrasbordo, Is.False);
        Assert.That(boleto.BoletoAnterior, Is.Null);
    }

    [Test]
    public void PropiedadesAlias_RetornanMismosValores()
    {
        var boleto = new Boleto("115", 2150m, 2850m, 5, FechaBaseLunes, "Normal", esTrasbordo: true);

        Assert.That(boleto.TotalAbonado, Is.EqualTo(boleto.MontoPagado));
        Assert.That(boleto.Saldo, Is.EqualTo(boleto.SaldoRestante));
        Assert.That(boleto.EsTransbordo, Is.EqualTo(boleto.EsTrasbordo));
    }

    [Test]
    public void ObtenerPrimerBoletoVentana_BoletoSinAnterior_RetornaSiMismo()
    {
        var boleto = new Boleto("102", 2150m, 2850m, 1, FechaBaseLunes);

        var primerBoleto = boleto.ObtenerPrimerBoletoVentana();

        Assert.That(primerBoleto, Is.SameAs(boleto));
    }

    [Test]
    public void ObtenerPrimerBoletoVentana_CadenaDeTrasbordos_RetornaBoletoOriginal()
    {
        var boleto1 = new Boleto("102", 2150m, 2850m, 1, FechaBaseLunes);
        var boleto2 = new Boleto("144", 0m, 2850m, 1, FechaBaseLunes.AddMinutes(20), "Normal", esTrasbordo: true, boletoAnterior: boleto1);
        var boleto3 = new Boleto("115", 0m, 2850m, 1, FechaBaseLunes.AddMinutes(40), "Normal", esTrasbordo: true, boletoAnterior: boleto2);

        var primerBoleto = boleto3.ObtenerPrimerBoletoVentana();

        Assert.That(primerBoleto, Is.SameAs(boleto1));
    }

    [Test]
    public void ContieneLineaEnVentana_DetectaLineasCorrectamente()
    {
        var boleto1 = new Boleto("102", 2150m, 2850m, 1, FechaBaseLunes);
        var boleto2 = new Boleto("144", 0m, 2850m, 1, FechaBaseLunes.AddMinutes(20), "Normal", esTrasbordo: true, boletoAnterior: boleto1);

        Assert.That(boleto2.ContieneLineaEnVentana("102"), Is.True);
        Assert.That(boleto2.ContieneLineaEnVentana("144"), Is.True);
        Assert.That(boleto2.ContieneLineaEnVentana("115"), Is.False);
    }

    [Test]
    public void EsTrasbordoValido_MismaLinea_RetornaFalse()
    {
        var boleto = new Boleto("102", 2150m, 2850m, 1, FechaBaseLunes);

        bool valido = boleto.EsTrasbordoValido("102", FechaBaseLunes.AddMinutes(15));

        Assert.That(valido, Is.False);
    }

    [Test]
    public void EsTrasbordoValido_Domingo_RetornaFalse()
    {
        var domingo = new DateTime(2026, 10, 25, 12, 0, 0);
        var boleto = new Boleto("102", 2150m, 2850m, 1, domingo);

        bool valido = boleto.EsTrasbordoValido("144", domingo.AddMinutes(15));

        Assert.That(valido, Is.False);
    }

    [Test]
    public void EsTrasbordoValido_FueraDeFranjaHoraria_RetornaFalse()
    {
        var boletoTemprano = new Boleto("102", 2150m, 2850m, 1, new DateTime(2026, 10, 19, 6, 30, 0));
        bool antesDeHora = boletoTemprano.EsTrasbordoValido("144", new DateTime(2026, 10, 19, 6, 59, 59));
        Assert.That(antesDeHora, Is.False);

        var boletoTarde = new Boleto("102", 2150m, 2850m, 1, new DateTime(2026, 10, 19, 21, 30, 0));
        bool despuesDeHora = boletoTarde.EsTrasbordoValido("144", new DateTime(2026, 10, 19, 22, 0, 1));
        Assert.That(despuesDeHora, Is.False);
    }

    [Test]
    public void EsTrasbordoValido_ExcediendoUnaHora_RetornaFalse()
    {
        var boleto = new Boleto("102", 2150m, 2850m, 1, FechaBaseLunes);

        bool valido = boleto.EsTrasbordoValido("144", FechaBaseLunes.AddMinutes(61));

        Assert.That(valido, Is.False);
    }

    [Test]
    public void EsTrasbordoValido_DentroDeVentanaValida_RetornaTrue()
    {
        var boleto = new Boleto("102", 2150m, 2850m, 1, FechaBaseLunes);

        bool valido = boleto.EsTrasbordoValido("144", FechaBaseLunes.AddMinutes(50));

        Assert.That(valido, Is.True);
    }
}
