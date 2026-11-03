namespace TarjetaSubeTest;

using NUnit.Framework;
using TarjetaSube;

[TestFixture]
public class ColectivoTest
{
    private static readonly DateTime FechaHabilValida = new(2026, 10, 19, 10, 0, 0); // Lunes 10:00
    private static readonly DateTime DomingoManiana = new(2026, 10, 25, 10, 0, 0);

    [Test]
    public void PagarCon_ConSaldoSuficiente_ViajeExitosoYDescuentaSaldo()
    {
        var colectivo = new Colectivo("102 Roja");
        var tarjeta = new Tarjeta(5000m);

        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m - Colectivo.TarifaBasica));
    }

    [Test]
    public void PagarCon_ConSaldoExacto_ViajeExitosoYSaldoQuedaEnCero()
    {
        var colectivo = new Colectivo("144 Negra");
        var tarjeta = new Tarjeta(Colectivo.TarifaBasica);

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
        Assert.That(boleto.MontoPagado, Is.EqualTo(Colectivo.TarifaBasica));
        Assert.That(boleto.SaldoRestante, Is.EqualTo(4000m - Colectivo.TarifaBasica));
        Assert.That(boleto.TarjetaId, Is.EqualTo(42));
        Assert.That(boleto.Fecha, Is.GreaterThanOrEqualTo(fechaAntes));
        Assert.That(boleto.Fecha, Is.LessThanOrEqualTo(fechaDespues));
    }

    [Test]
    public void PagarCon_ConSaldoInsuficiente_RetornaNullYNoModificaSaldo()
    {
        var colectivo = new Colectivo("102 Roja");
        var tarjeta = new Tarjeta(500m);
        colectivo.pagarCon(tarjeta);

        var segundoBoleto = colectivo.pagarCon(tarjeta);

        Assert.That(segundoBoleto, Is.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(500m - Colectivo.TarifaBasica));
    }

    [Test]
    public void PagarCon_ConSaldoMenorATarifa_PermiteViajePlusYSaldoQuedaNegativo()
    {
        var colectivo = new Colectivo("K");
        var tarjeta = new Tarjeta(500m);

        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(500m - Colectivo.TarifaBasica));
    }

    [Test]
    public void PagarCon_ViajesConsecutivos_TercerViajeFallaPorSuperarLimiteNegativo()
    {
        var colectivo = new Colectivo("115");
        var tarjeta = new Tarjeta(3000m);

        var primerBoleto = colectivo.pagarCon(tarjeta);
        Assert.That(primerBoleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(850m));

        var segundoBoleto = colectivo.pagarCon(tarjeta);
        Assert.That(segundoBoleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-1300m));

        var tercerBoleto = colectivo.pagarCon(tarjeta);
        Assert.That(tercerBoleto, Is.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-1300m));
    }

    [Test]
    public void PagarCon_MultiplesViajesConsecutivosConSaldoSuficiente_DescuentaSaldoAcumulado()
    {
        var colectivo = new Colectivo("102 Roja");
        var tarjeta = new Tarjeta(5000m);

        var primerBoleto = colectivo.PagarCon(tarjeta);
        Assert.That(primerBoleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(2850m));
        Assert.That(primerBoleto!.SaldoRestante, Is.EqualTo(2850m));

        var segundoBoleto = colectivo.PagarCon(tarjeta);
        Assert.That(segundoBoleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(700m));
        Assert.That(segundoBoleto!.SaldoRestante, Is.EqualTo(700m));
    }

    [Test]
    public void PagarCon_TarjetaNull_RetornaNull()
    {
        var colectivo = new Colectivo("102");

        var boleto = colectivo.pagarCon(null!);

        Assert.That(boleto, Is.Null);
    }

    [Test]
    public void Constructor_PorDefecto_InicializaValoresPredeterminados()
    {
        var colectivo = new Colectivo();

        Assert.That(colectivo.Linea, Is.Empty);
        Assert.That(colectivo.EsInterurbano, Is.False);
        Assert.That(colectivo.Tarifa, Is.EqualTo(Colectivo.TarifaBasica));
    }

    [Test]
    public void Constructor_AsignaLineaCorrectamente()
    {
        var colectivo = new Colectivo("Línea de la Costa");

        Assert.That(colectivo.Linea, Is.EqualTo("Línea de la Costa"));
        Assert.That(colectivo.EsInterurbano, Is.False);
        Assert.That(colectivo.Tarifa, Is.EqualTo(2150m));
        Assert.That(Colectivo.TarifaBasica, Is.EqualTo(2150m));
    }

    [Test]
    public void Constructor_ConEsInterurbano_AsignaTarifaInterurbana()
    {
        var colectivo = new Colectivo("35/9", esInterurbano: true);

        Assert.That(colectivo.Linea, Is.EqualTo("35/9"));
        Assert.That(colectivo.EsInterurbano, Is.True);
        Assert.That(colectivo.Tarifa, Is.EqualTo(Colectivo.TarifaInterurbana));
        Assert.That(colectivo.Tarifa, Is.EqualTo(3000m));
    }

    [Test]
    public void Colectivo_DistingueEntreUrbanoEInterurbano()
    {
        var urbano = new Colectivo("144 Negra");
        var interurbanoPorFlag = new Colectivo("Rosario - Baigorria", esInterurbano: true);
        var interurbanoClase = new ColectivoInterurbano("Rosario - Pérez");

        Assert.That(urbano.EsInterurbano, Is.False);
        Assert.That(urbano.Tarifa, Is.EqualTo(2150m));

        Assert.That(interurbanoPorFlag.EsInterurbano, Is.True);
        Assert.That(interurbanoPorFlag.Tarifa, Is.EqualTo(3000m));

        Assert.That(interurbanoClase.EsInterurbano, Is.True);
        Assert.That(interurbanoClase.Tarifa, Is.EqualTo(3000m));
    }

    [Test]
    public void ColectivoInterurbano_ConstructorPorDefecto_ConfiguraTarifa3000()
    {
        var interurbano = new ColectivoInterurbano();

        Assert.That(interurbano.EsInterurbano, Is.True);
        Assert.That(interurbano.Tarifa, Is.EqualTo(3000m));
        Assert.That(ColectivoInterurbano.TarifaInterurbanaFija, Is.EqualTo(3000m));
    }

    [Test]
    public void ColectivoInterurbano_ConstructorConLinea_AsignaLineaYTarifaCorrecta()
    {
        var interurbano = new ColectivoInterurbano("Expreso");

        Assert.That(interurbano.Linea, Is.EqualTo("Expreso"));
        Assert.That(interurbano.EsInterurbano, Is.True);
        Assert.That(interurbano.Tarifa, Is.EqualTo(3000m));
    }

    [Test]
    public void ColectivoInterurbano_PagarCon_DescuentaTarifa3000()
    {
        var interurbano = new ColectivoInterurbano("M");
        var tarjeta = new Tarjeta(5000m);

        var boleto = interurbano.PagarCon(tarjeta, FechaHabilValida);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.MontoPagado, Is.EqualTo(3000m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(2000m));
    }

    [Test]
    public void ColectivoInterurbano_PagarCon_MedioBoleto_DescuentaLaMitad()
    {
        var interurbano = new ColectivoInterurbano("33/9");
        var tarjeta = new Tarjeta(5000m, TarjetaTipo.MedioBoleto);

        var boleto = interurbano.PagarCon(tarjeta, FechaHabilValida);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.MontoPagado, Is.EqualTo(1500m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(3500m));
    }

    [Test]
    public void FranquiciaCompleta_EnLineaInterurbana_PrimerosDosViajesSonGratis()
    {
        var interurbano = new ColectivoInterurbano("Rosario - Pérez");
        var tarjeta = new Tarjeta(0m, TarjetaTipo.FranquiciaCompleta);

        var boleto1 = interurbano.PagarCon(tarjeta, FechaHabilValida);
        Assert.That(boleto1, Is.Not.Null);
        Assert.That(boleto1!.TotalAbonado, Is.EqualTo(0m));

        var boleto2 = interurbano.PagarCon(tarjeta, FechaHabilValida.AddMinutes(10));
        Assert.That(boleto2, Is.Not.Null);
        Assert.That(boleto2!.TotalAbonado, Is.EqualTo(0m));

        var boleto3 = interurbano.PagarCon(tarjeta, FechaHabilValida.AddMinutes(20));
        Assert.That(boleto3, Is.Null);
    }

    [Test]
    public void FranquiciaCompleta_SiemprePuedePagarUnBoleto_InclusoConSaldoCero()
    {
        var interurbano = new ColectivoInterurbano("Rosario - Gálvez");
        var urbano = new Colectivo("144");
        var tarjetaCero = new Tarjeta(0m, TarjetaTipo.FranquiciaCompleta);

        var boletoUrbano = urbano.PagarCon(tarjetaCero, FechaHabilValida);
        Assert.That(boletoUrbano, Is.Not.Null);
        Assert.That(boletoUrbano!.TotalAbonado, Is.EqualTo(0m));

        var boletoInterurbano = interurbano.PagarCon(tarjetaCero, FechaHabilValida.AddMinutes(10));
        Assert.That(boletoInterurbano, Is.Not.Null);
        Assert.That(boletoInterurbano!.TotalAbonado, Is.EqualTo(0m));
    }

    [Test]
    public void TarifaInterurbana_ConSaldoNegativo_PermiteViajeHastaLimiteNegativo()
    {
        var interurbano = new ColectivoInterurbano("Rosario - Baigorria");
        var tarjeta = new Tarjeta(1000m);

        var boleto = interurbano.PagarCon(tarjeta, FechaHabilValida);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-2000m));
        Assert.That(tarjeta.ViajesPlus, Is.EqualTo(1));

        var segundoBoleto = interurbano.PagarCon(tarjeta, FechaHabilValida.AddMinutes(10));
        Assert.That(segundoBoleto, Is.Null);
    }

    [Test]
    public void Franquicias_EnLineaInterurbana_RespetanFranjaHoraria()
    {
        var interurbano = new ColectivoInterurbano("Rosario - Pérez");
        var tarjeta = new Tarjeta(5000m, TarjetaTipo.MedioBoleto);

        var boletoDomingo = interurbano.PagarCon(tarjeta, DomingoManiana);
        Assert.That(boletoDomingo, Is.Null);

        var lunesMadrugada = new DateTime(2026, 10, 19, 5, 30, 0);
        var boletoMadrugada = interurbano.PagarCon(tarjeta, lunesMadrugada);
        Assert.That(boletoMadrugada, Is.Null);
    }

    [Test]
    public void Trasbordo_EntreLineasDistintasDentroDeLaHora_EsGratuito()
    {
        var colectivo1 = new Colectivo("102");
        var colectivo2 = new Colectivo("144");
        var tarjeta = new Tarjeta(5000m);

        var boleto1 = colectivo1.PagarCon(tarjeta, FechaHabilValida);
        Assert.That(boleto1, Is.Not.Null);
        Assert.That(boleto1!.EsTrasbordo, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m - 2150m));

        var boleto2 = colectivo2.PagarCon(tarjeta, FechaHabilValida.AddMinutes(30));
        Assert.That(boleto2, Is.Not.Null);
        Assert.That(boleto2!.EsTrasbordo, Is.True);
        Assert.That(boleto2.MontoPagado, Is.EqualTo(0m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m - 2150m));
    }

    [Test]
    public void Trasbordo_HaciaColectivoInterurbanoDentroDeLaHora_EsGratuito()
    {
        var urbano = new Colectivo("115");
        var interurbano = new ColectivoInterurbano("Expreso");
        var tarjeta = new Tarjeta(5000m);

        var boleto1 = urbano.PagarCon(tarjeta, FechaHabilValida);
        Assert.That(boleto1, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(2850m));

        var boleto2 = interurbano.PagarCon(tarjeta, FechaHabilValida.AddMinutes(40));
        Assert.That(boleto2, Is.Not.Null);
        Assert.That(boleto2!.EsTrasbordo, Is.True);
        Assert.That(boleto2.MontoPagado, Is.EqualTo(0m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(2850m));
    }

    [Test]
    public void Trasbordo_MultipleCantidadDeTrasbordosDentroDeLaMismaHora()
    {
        var colectivoA = new Colectivo("Línea A");
        var colectivoB = new Colectivo("Línea B");
        var colectivoC = new Colectivo("Línea C");
        var colectivoD = new Colectivo("Línea D");
        var tarjeta = new Tarjeta(10000m);

        var inicio = new DateTime(2026, 10, 19, 8, 0, 0);

        var b1 = colectivoA.PagarCon(tarjeta, inicio);
        Assert.That(b1!.TotalAbonado, Is.EqualTo(2150m));

        var b2 = colectivoB.PagarCon(tarjeta, inicio.AddMinutes(15));
        Assert.That(b2!.EsTrasbordo, Is.True);
        Assert.That(b2.TotalAbonado, Is.EqualTo(0m));

        var b3 = colectivoC.PagarCon(tarjeta, inicio.AddMinutes(35));
        Assert.That(b3!.EsTrasbordo, Is.True);
        Assert.That(b3.TotalAbonado, Is.EqualTo(0m));

        var b4 = colectivoD.PagarCon(tarjeta, inicio.AddMinutes(55));
        Assert.That(b4!.EsTrasbordo, Is.True);
        Assert.That(b4.TotalAbonado, Is.EqualTo(0m));

        Assert.That(tarjeta.Saldo, Is.EqualTo(7850m));
    }

    [Test]
    public void Trasbordo_EnLimiteExactoDeUnaHora_EsValido()
    {
        var colectivo1 = new Colectivo("102 Roja");
        var colectivo2 = new Colectivo("144 Negra");
        var tarjeta = new Tarjeta(5000m);

        var fecha1 = new DateTime(2026, 10, 19, 10, 0, 0);
        var fecha2 = fecha1.AddHours(1);

        colectivo1.PagarCon(tarjeta, fecha1);
        var boleto2 = colectivo2.PagarCon(tarjeta, fecha2);

        Assert.That(boleto2, Is.Not.Null);
        Assert.That(boleto2!.EsTrasbordo, Is.True);
        Assert.That(boleto2.TotalAbonado, Is.EqualTo(0m));
    }

    [Test]
    public void Trasbordo_EnSabados_EstaHabilitadoEntre7Y22()
    {
        var col1 = new Colectivo("115");
        var col2 = new Colectivo("122 Verde");
        var tarjeta = new Tarjeta(5000m);

        var sabado1 = new DateTime(2026, 10, 24, 14, 0, 0);
        var sabado2 = new DateTime(2026, 10, 24, 14, 30, 0);

        var b1 = col1.PagarCon(tarjeta, sabado1);
        var b2 = col2.PagarCon(tarjeta, sabado2);

        Assert.That(b1, Is.Not.Null);
        Assert.That(b2, Is.Not.Null);
        Assert.That(b2!.EsTrasbordo, Is.True);
        Assert.That(b2.TotalAbonado, Is.EqualTo(0m));
    }

    [Test]
    public void Trasbordo_EntreDosLineasInterurbanasDistintas_EsGratuito()
    {
        var interurbano1 = new ColectivoInterurbano("Rosario - Gálvez");
        var interurbano2 = new ColectivoInterurbano("Rosario - Baigorria");
        var tarjeta = new Tarjeta(10000m);

        var t1 = new DateTime(2026, 10, 19, 9, 0, 0);
        var t2 = new DateTime(2026, 10, 19, 9, 20, 0);

        var b1 = interurbano1.PagarCon(tarjeta, t1);
        Assert.That(b1!.TotalAbonado, Is.EqualTo(3000m));

        var b2 = interurbano2.PagarCon(tarjeta, t2);
        Assert.That(b2!.EsTrasbordo, Is.True);
        Assert.That(b2.TotalAbonado, Is.EqualTo(0m));
    }

    [Test]
    public void Trasbordo_ConTarjetaConSaldoNegativo_PermiteTrasbordoSinCosto()
    {
        var col1 = new Colectivo("102");
        var col2 = new Colectivo("144");
        var tarjeta = new Tarjeta(500m);

        var t1 = new DateTime(2026, 10, 19, 10, 0, 0);
        var t2 = new DateTime(2026, 10, 19, 10, 20, 0);

        col1.PagarCon(tarjeta, t1);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-1650m));

        var b2 = col2.PagarCon(tarjeta, t2);
        Assert.That(b2, Is.Not.Null);
        Assert.That(b2!.EsTrasbordo, Is.True);
        Assert.That(b2.TotalAbonado, Is.EqualTo(0m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(-1650m));
    }

    [Test]
    public void RechazoTrasbordo_MismaLinea_CobraBoletoNormalYEsTrasbordoEsFalse()
    {
        var colectivo1 = new Colectivo("102 Roja");
        var colectivo2 = new Colectivo("102 Roja");
        var tarjeta = new Tarjeta(5000m);

        var t1 = new DateTime(2026, 10, 19, 10, 0, 0);
        var t2 = new DateTime(2026, 10, 19, 10, 20, 0);

        var b1 = colectivo1.PagarCon(tarjeta, t1);
        Assert.That(b1!.TotalAbonado, Is.EqualTo(2150m));

        var b2 = colectivo2.PagarCon(tarjeta, t2);
        Assert.That(b2, Is.Not.Null);
        Assert.That(b2!.EsTrasbordo, Is.False);
        Assert.That(b2.TotalAbonado, Is.EqualTo(2150m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(700m));
    }

    [Test]
    public void RechazoTrasbordo_MismaLineaRepetidaEnLaVentana_NoEsTrasbordoGratis()
    {
        var colA = new Colectivo("Línea 102");
        var colB = new Colectivo("Línea 144");
        var colARegreso = new Colectivo("Línea 102");
        var tarjeta = new Tarjeta(10000m);

        var t1 = new DateTime(2026, 10, 19, 10, 0, 0);
        var t2 = new DateTime(2026, 10, 19, 10, 20, 0);
        var t3 = new DateTime(2026, 10, 19, 10, 40, 0);

        var b1 = colA.PagarCon(tarjeta, t1);
        var b2 = colB.PagarCon(tarjeta, t2);
        Assert.That(b2!.EsTrasbordo, Is.True);

        var b3 = colARegreso.PagarCon(tarjeta, t3);
        Assert.That(b3, Is.Not.Null);
        Assert.That(b3!.EsTrasbordo, Is.False);
        Assert.That(b3.TotalAbonado, Is.EqualTo(2150m));
    }

    [Test]
    public void RechazoTrasbordo_DespuesDeUnaHora_CobraTarifaNormal()
    {
        var col1 = new Colectivo("102 Roja");
        var col2 = new Colectivo("144 Negra");
        var tarjeta = new Tarjeta(5000m);

        var t1 = new DateTime(2026, 10, 19, 10, 0, 0);
        var t2 = new DateTime(2026, 10, 19, 11, 0, 1);

        var b1 = col1.PagarCon(tarjeta, t1);
        Assert.That(b1!.TotalAbonado, Is.EqualTo(2150m));

        var b2 = col2.PagarCon(tarjeta, t2);
        Assert.That(b2, Is.Not.Null);
        Assert.That(b2!.EsTrasbordo, Is.False);
        Assert.That(b2.TotalAbonado, Is.EqualTo(2150m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(700m));
    }

    [Test]
    public void RechazoTrasbordo_EnDomingo_CobraTarifaNormal()
    {
        var col1 = new Colectivo("102 Roja");
        var col2 = new Colectivo("144 Negra");
        var tarjeta = new Tarjeta(5000m);

        var domingo1 = new DateTime(2026, 10, 25, 10, 0, 0);
        var domingo2 = new DateTime(2026, 10, 25, 10, 20, 0);

        var b1 = col1.PagarCon(tarjeta, domingo1);
        Assert.That(b1!.TotalAbonado, Is.EqualTo(2150m));

        var b2 = col2.PagarCon(tarjeta, domingo2);
        Assert.That(b2, Is.Not.Null);
        Assert.That(b2!.EsTrasbordo, Is.False);
        Assert.That(b2.TotalAbonado, Is.EqualTo(2150m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(700m));
    }

    [Test]
    public void RechazoTrasbordo_FueraDeHorario_AntesDeLas07_CobraTarifaNormal()
    {
        var col1 = new Colectivo("102");
        var col2 = new Colectivo("144");
        var tarjeta = new Tarjeta(5000m);

        var t1 = new DateTime(2026, 10, 19, 6, 15, 0);
        var t2 = new DateTime(2026, 10, 19, 6, 45, 0);

        var b1 = col1.PagarCon(tarjeta, t1);
        var b2 = col2.PagarCon(tarjeta, t2);

        Assert.That(b2, Is.Not.Null);
        Assert.That(b2!.EsTrasbordo, Is.False);
        Assert.That(b2.TotalAbonado, Is.EqualTo(2150m));
    }

    [Test]
    public void RechazoTrasbordo_FueraDeHorario_DespuesDeLas22_CobraTarifaNormal()
    {
        var col1 = new Colectivo("102");
        var col2 = new Colectivo("144");
        var tarjeta = new Tarjeta(5000m);

        var t1 = new DateTime(2026, 10, 19, 21, 50, 0);
        var t2 = new DateTime(2026, 10, 19, 22, 15, 0);

        var b1 = col1.PagarCon(tarjeta, t1);
        var b2 = col2.PagarCon(tarjeta, t2);

        Assert.That(b2, Is.Not.Null);
        Assert.That(b2!.EsTrasbordo, Is.False);
        Assert.That(b2.TotalAbonado, Is.EqualTo(2150m));
    }

    [Test]
    public void RechazoTrasbordoPorTiempo_IniciaNuevaVentanaParaFuturosTrasbordos()
    {
        var colA = new Colectivo("Línea A");
        var colB = new Colectivo("Línea B");
        var colC = new Colectivo("Línea C");
        var tarjeta = new Tarjeta(10000m);

        var t1 = new DateTime(2026, 10, 19, 8, 0, 0);
        var t2 = new DateTime(2026, 10, 19, 9, 10, 0);
        var t3 = new DateTime(2026, 10, 19, 9, 30, 0);

        var b1 = colA.PagarCon(tarjeta, t1);
        Assert.That(b1!.TotalAbonado, Is.EqualTo(2150m));

        var b2 = colB.PagarCon(tarjeta, t2);
        Assert.That(b2!.EsTrasbordo, Is.False);
        Assert.That(b2.TotalAbonado, Is.EqualTo(2150m));

        var b3 = colC.PagarCon(tarjeta, t3);
        Assert.That(b3!.EsTrasbordo, Is.True);
        Assert.That(b3.TotalAbonado, Is.EqualTo(0m));
    }

    [Test]
    public void Franquicia_DiaInhabil_RechazaPagoRetornandoNull()
    {
        var domingo = new DateTime(2026, 10, 25, 14, 0, 0);
        var colectivo = new Colectivo("102");
        var tarjeta = new Tarjeta(5000m, TarjetaTipo.MedioBoleto);

        var boleto = colectivo.PagarCon(tarjeta, domingo);

        Assert.That(boleto, Is.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m));
    }

    [Test]
    public void Franquicia_FueraDeHorario_RechazaPagoRetornandoNull()
    {
        var colectivo = new Colectivo("102");
        var tarjeta = new Tarjeta(5000m, TarjetaTipo.FranquiciaCompleta);

        var temprano = new DateTime(2026, 10, 19, 5, 59, 59);
        var boletoTemprano = colectivo.PagarCon(tarjeta, temprano);
        Assert.That(boletoTemprano, Is.Null);

        var tarde = new DateTime(2026, 10, 19, 22, 0, 1);
        var boletoTarde = colectivo.PagarCon(tarjeta, tarde);
        Assert.That(boletoTarde, Is.Null);

        var enHorario = new DateTime(2026, 10, 19, 6, 0, 0);
        var boletoEnHorario = colectivo.PagarCon(tarjeta, enHorario);
        Assert.That(boletoEnHorario, Is.Not.Null);
    }

    [Test]
    public void TodasLasFranquicias_RespetanLaFranjaHoraria()
    {
        var colectivo = new Colectivo("144");
        var domingo = new DateTime(2026, 10, 25, 10, 0, 0);

        var tarjetaMedio = new Tarjeta(5000m, TarjetaTipo.MedioBoleto);
        var tarjetaCompleta = new Tarjeta(5000m, TarjetaTipo.FranquiciaCompleta);
        var tarjetaGratuito = new Tarjeta(5000m, TarjetaTipo.BoletoGratuito);
        var tarjetaSocial = new Tarjeta(5000m, new TarjetaTipo("Tarifa Social", 0.45m));

        Assert.That(colectivo.PagarCon(tarjetaMedio, domingo), Is.Null);
        Assert.That(colectivo.PagarCon(tarjetaCompleta, domingo), Is.Null);
        Assert.That(colectivo.PagarCon(tarjetaGratuito, domingo), Is.Null);
        Assert.That(colectivo.PagarCon(tarjetaSocial, domingo), Is.Null);
    }

    [Test]
    public void TarjetaNormal_SinRestriccionHorariaNiDeDia_ViajaEnCualquierMomento()
    {
        var colectivo = new Colectivo("102");
        var tarjeta = new Tarjeta(10000m);

        var domingoMadrugada = new DateTime(2026, 10, 25, 3, 30, 0);
        var boleto = colectivo.PagarCon(tarjeta, domingoMadrugada);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(10000m - 2150m));
    }
}
