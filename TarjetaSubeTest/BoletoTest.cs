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
    public void Boleto_PermiteConocerTodosLosDatosRequeridos_MediantePropiedadesYMetodos()
    {
        var tipo = new TarjetaTipo(TarjetaTipo.NombreNormal, 1m);
        var tarjeta = new Tarjeta(5000m, tipo) { Id = 42 };
        var colectivo = new Colectivo("144 Negra");
        var fecha = new DateTime(2026, 10, 20, 14, 30, 0);

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Fecha, Is.EqualTo(fecha));
        Assert.That(boleto.ObtenerFecha(), Is.EqualTo(fecha));
        Assert.That(boleto.TipoTarjeta, Is.EqualTo(TarjetaTipo.NombreNormal));
        Assert.That(boleto.ObtenerTipoTarjeta(), Is.EqualTo(TarjetaTipo.NombreNormal));
        Assert.That(boleto.Linea, Is.EqualTo("144 Negra"));
        Assert.That(boleto.ObtenerLinea(), Is.EqualTo("144 Negra"));
        Assert.That(boleto.TotalAbonado, Is.EqualTo(Colectivo.TarifaBasica));
        Assert.That(boleto.MontoPagado, Is.EqualTo(Colectivo.TarifaBasica));
        Assert.That(boleto.ObtenerTotalAbonado(), Is.EqualTo(Colectivo.TarifaBasica));

        decimal saldoEsperado = 5000m - Colectivo.TarifaBasica;
        Assert.That(boleto.Saldo, Is.EqualTo(saldoEsperado));
        Assert.That(boleto.SaldoRestante, Is.EqualTo(saldoEsperado));
        Assert.That(boleto.ObtenerSaldo(), Is.EqualTo(saldoEsperado));
        Assert.That(boleto.TarjetaId, Is.EqualTo(42));
        Assert.That(boleto.ObtenerTarjetaId(), Is.EqualTo(42));
    }

    [Test]
    public void Boleto_PosiblesTiposDeTarjeta_GeneranBoletosCorrectos()
    {
        var colectivo = new Colectivo("102 Roja");
        var fecha = new DateTime(2026, 10, 20, 10, 0, 0);

        var tarjetaNormal = new Tarjeta(5000m, new TarjetaTipo(TarjetaTipo.NombreNormal, 1m)) { Id = 1 };
        var boletoNormal = colectivo.PagarCon(tarjetaNormal, fecha);
        Assert.That(boletoNormal!.ObtenerTipoTarjeta(), Is.EqualTo(TarjetaTipo.NombreNormal));
        Assert.That(boletoNormal.ObtenerTotalAbonado(), Is.EqualTo(Colectivo.TarifaBasica));

        var tarjetaMedio = new Tarjeta(5000m, new TarjetaTipo(TarjetaTipo.NombreMedioBoleto, 0.5m)) { Id = 2 };
        var boletoMedio = colectivo.PagarCon(tarjetaMedio, fecha);
        Assert.That(boletoMedio!.ObtenerTipoTarjeta(), Is.EqualTo(TarjetaTipo.NombreMedioBoleto));
        Assert.That(boletoMedio.ObtenerTotalAbonado(), Is.EqualTo(Colectivo.TarifaBasica / 2m));

        var tarjetaFranquicia = new Tarjeta(5000m, new TarjetaTipo(TarjetaTipo.NombreFranquiciaCompleta, 0m)) { Id = 3 };
        var boletoFranquicia = colectivo.PagarCon(tarjetaFranquicia, fecha);
        Assert.That(boletoFranquicia!.ObtenerTipoTarjeta(), Is.EqualTo(TarjetaTipo.NombreFranquiciaCompleta));
        Assert.That(boletoFranquicia.ObtenerTotalAbonado(), Is.EqualTo(0m));

        var tarjetaGratuito = new Tarjeta(5000m, new TarjetaTipo(TarjetaTipo.NombreBoletoGratuito, 0m)) { Id = 4 };
        var boletoGratuito = colectivo.PagarCon(tarjetaGratuito, fecha);
        Assert.That(boletoGratuito!.ObtenerTipoTarjeta(), Is.EqualTo(TarjetaTipo.NombreBoletoGratuito));
        Assert.That(boletoGratuito.ObtenerTotalAbonado(), Is.EqualTo(0m));
    }

    [Test]
    public void Boleto_ConSaldoNegativoPrevio_InformaTotalAbonadoSuperiorAlCostoDeSuViaje()
    {
        var tipoMedio = new TarjetaTipo(TarjetaTipo.NombreMedioBoleto, 0.5m);
        var tarjeta = new Tarjeta(1000m, tipoMedio) { Id = 99 };
        var colectivo = new Colectivo("115");
        var fecha1 = new DateTime(2026, 10, 20, 8, 0, 0);
        var fecha2 = new DateTime(2026, 10, 20, 12, 0, 0);

        var primerBoleto = colectivo.PagarCon(tarjeta, fecha1);
        Assert.That(primerBoleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-75m));
        Assert.That(primerBoleto!.TotalAbonado, Is.EqualTo(Colectivo.TarifaBasica / 2m));

        var segundoBoleto = colectivo.PagarCon(tarjeta, fecha2);

        Assert.That(segundoBoleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-1150m));
        Assert.That(segundoBoleto!.TotalAbonado, Is.EqualTo(1150m));
        Assert.That(segundoBoleto.TotalAbonado, Is.GreaterThan(Colectivo.TarifaBasica / 2m));
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
