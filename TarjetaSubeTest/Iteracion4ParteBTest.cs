namespace TarjetaSubeTest;

using NUnit.Framework;
using TarjetaSube;

[TestFixture]
public class Iteracion4ParteBTest
{
    private static readonly DateTime LunesManiana = new(2026, 10, 19, 10, 0, 0);
    private static readonly DateTime SabadoTarde = new(2026, 10, 24, 15, 0, 0);
    private static readonly DateTime DomingoManiana = new(2026, 10, 25, 10, 0, 0);

    [Test]
    public void ColectivoInterurbano_TieneTarifaDeTresMilPesos()
    {
        var interurbano = new ColectivoInterurbano("Rosario - Gálvez");

        Assert.That(Colectivo.TarifaInterurbana, Is.EqualTo(3000m));
        Assert.That(ColectivoInterurbano.TarifaInterurbanaFija, Is.EqualTo(3000m));
        Assert.That(interurbano.Tarifa, Is.EqualTo(3000m));
        Assert.That(interurbano.EsInterurbano, Is.True);
        Assert.That(interurbano.Linea, Is.EqualTo("Rosario - Gálvez"));
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
    public void TarjetaNormal_PagaTarifaInterurbanaCompletaDeTresMil()
    {
        var interurbano = new ColectivoInterurbano("Rosario - Gálvez");
        var tarjeta = new Tarjeta(10000m);

        var boleto = interurbano.PagarCon(tarjeta, LunesManiana);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.TotalAbonado, Is.EqualTo(3000m));
        Assert.That(boleto.MontoPagado, Is.EqualTo(3000m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(7000m));
        Assert.That(boleto.SaldoRestante, Is.EqualTo(7000m));
        Assert.That(boleto.EsTrasbordo, Is.False);
    }

    [Test]
    public void MedioBoleto_EnLineaInterurbana_PagaLaMitadDeTresMil()
    {
        var interurbano = new ColectivoInterurbano("Rosario - Baigorria");
        var tipoMedio = new TarjetaTipo(TarjetaTipo.NombreMedioBoleto, 0.5m);
        var tarjeta = new Tarjeta(5000m, tipoMedio);

        var boleto1 = interurbano.PagarCon(tarjeta, LunesManiana);
        Assert.That(boleto1, Is.Not.Null);
        Assert.That(boleto1!.TotalAbonado, Is.EqualTo(1500m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(3500m));

        var boleto2 = interurbano.PagarCon(tarjeta, LunesManiana.AddMinutes(10));
        Assert.That(boleto2, Is.Not.Null);
        Assert.That(boleto2!.TotalAbonado, Is.EqualTo(1500m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(2000m));

        var boleto3 = interurbano.PagarCon(tarjeta, LunesManiana.AddMinutes(20));
        Assert.That(boleto3, Is.Not.Null);
        Assert.That(boleto3!.TotalAbonado, Is.EqualTo(3000m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(-1000m));
    }

    [Test]
    public void FranquiciaCompleta_EnLineaInterurbana_PrimerosDosViajesSonGratis()
    {
        var interurbano = new ColectivoInterurbano("Rosario - Pérez");
        var tipoCompleta = new TarjetaTipo(TarjetaTipo.NombreFranquiciaCompleta, 0m);
        var tarjeta = new Tarjeta(0m, tipoCompleta);

        var boleto1 = interurbano.PagarCon(tarjeta, LunesManiana);
        Assert.That(boleto1, Is.Not.Null);
        Assert.That(boleto1!.TotalAbonado, Is.EqualTo(0m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(0m));

        var boleto2 = interurbano.PagarCon(tarjeta, LunesManiana.AddMinutes(10));
        Assert.That(boleto2, Is.Not.Null);
        Assert.That(boleto2!.TotalAbonado, Is.EqualTo(0m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(0m));

        var boleto3 = interurbano.PagarCon(tarjeta, LunesManiana.AddMinutes(20));
        Assert.That(boleto3, Is.Null);

        tarjeta.CargarSaldo(5000m);
        var boleto3ConSaldo = interurbano.PagarCon(tarjeta, LunesManiana.AddMinutes(25));
        Assert.That(boleto3ConSaldo, Is.Not.Null);
        Assert.That(boleto3ConSaldo!.TotalAbonado, Is.EqualTo(3000m));
    }

    [Test]
    public void FranquiciaCompleta_SiemprePuedePagarUnBoleto_InclusoConSaldoCero()
    {
        var interurbano = new ColectivoInterurbano("Rosario - Gálvez");
        var urbano = new Colectivo("144");
        var tarjetaCero = new Tarjeta(0m, new TarjetaTipo(TarjetaTipo.NombreFranquiciaCompleta, 0m));

        var boletoUrbano = urbano.PagarCon(tarjetaCero, LunesManiana);
        Assert.That(boletoUrbano, Is.Not.Null);
        Assert.That(boletoUrbano!.TotalAbonado, Is.EqualTo(0m));

        var boletoInterurbano = interurbano.PagarCon(tarjetaCero, LunesManiana.AddMinutes(10));
        Assert.That(boletoInterurbano, Is.Not.Null);
        Assert.That(boletoInterurbano!.TotalAbonado, Is.EqualTo(0m));
    }

    [Test]
    public void MedioBoleto_MontoPagadoEsSiempreLaMitadDelNormal()
    {
        var urbano = new Colectivo("102 Roja");
        var interurbano = new ColectivoInterurbano("Rosario - Funes");
        var tipoMedio = new TarjetaTipo(TarjetaTipo.NombreMedioBoleto, 0.5m);

        var tarjeta1 = new Tarjeta(5000m, tipoMedio);
        var boletoUrbano = urbano.PagarCon(tarjeta1, LunesManiana);
        Assert.That(boletoUrbano!.MontoPagado, Is.EqualTo(1075m));

        var tarjeta2 = new Tarjeta(5000m, tipoMedio);
        var boletoInterurbano = interurbano.PagarCon(tarjeta2, LunesManiana);
        Assert.That(boletoInterurbano!.MontoPagado, Is.EqualTo(1500m));
    }

    [Test]
    public void BoletoUsoFrecuente_AplicaDescuentosEnTarifaInterurbana()
    {
        var tarjeta = new Tarjeta(50000m);
        var fechaBase = new DateTime(2026, 10, 1, 8, 0, 0);

        for (int i = 0; i < 29; i++)
        {
            tarjeta.RegistrarViaje(fechaBase.AddHours(i));
        }

        decimal tarifaViaje30 = tarjeta.CalcularPrecio(Colectivo.TarifaInterurbana, fechaBase);
        Assert.That(tarifaViaje30, Is.EqualTo(2400m));

        for (int i = 29; i < 79; i++)
        {
            tarjeta.RegistrarViaje(fechaBase.AddHours(i));
        }

        decimal tarifaViaje80 = tarjeta.CalcularPrecio(Colectivo.TarifaInterurbana, fechaBase);
        Assert.That(tarifaViaje80, Is.EqualTo(2250m));
    }

    [Test]
    public void TarifaInterurbana_ConSaldoNegativo_PermiteViajeHastaLimiteNegativo()
    {
        var interurbano = new ColectivoInterurbano("Rosario - Baigorria");
        var tarjeta = new Tarjeta(1000m);

        var boleto = interurbano.PagarCon(tarjeta, LunesManiana);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-2000m));
        Assert.That(tarjeta.ViajesPlus, Is.EqualTo(1));

        var segundoBoleto = interurbano.PagarCon(tarjeta, LunesManiana.AddMinutes(10));
        Assert.That(segundoBoleto, Is.Null);
    }

    [Test]
    public void Franquicias_EnLineaInterurbana_RespetanFranjaHoraria()
    {
        var interurbano = new ColectivoInterurbano("Rosario - Pérez");
        var tipoMedio = new TarjetaTipo(TarjetaTipo.NombreMedioBoleto, 0.5m);
        var tarjeta = new Tarjeta(5000m, tipoMedio);

        var boletoDomingo = interurbano.PagarCon(tarjeta, DomingoManiana);
        Assert.That(boletoDomingo, Is.Null);

        var lunesMadrugada = new DateTime(2026, 10, 19, 5, 30, 0);
        var boletoMadrugada = interurbano.PagarCon(tarjeta, lunesMadrugada);
        Assert.That(boletoMadrugada, Is.Null);
    }

    [Test]
    public void TrasbordoValido_DistintaLineaDentroDeUnaHora_EsGratuitoYEmitenBoletoIndicador()
    {
        var colectivo1 = new Colectivo("102 Roja");
        var colectivo2 = new Colectivo("144 Negra");
        var tarjeta = new Tarjeta(5000m);

        var fechaPrimerViaje = new DateTime(2026, 10, 19, 10, 0, 0);
        var fechaTrasbordo = new DateTime(2026, 10, 19, 10, 25, 0);

        var boleto1 = colectivo1.PagarCon(tarjeta, fechaPrimerViaje);
        Assert.That(boleto1, Is.Not.Null);
        Assert.That(boleto1!.EsTrasbordo, Is.False);
        Assert.That(boleto1.TotalAbonado, Is.EqualTo(2150m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(2850m));

        var boleto2 = colectivo2.PagarCon(tarjeta, fechaTrasbordo);
        Assert.That(boleto2, Is.Not.Null);
        Assert.That(boleto2!.EsTrasbordo, Is.True);
        Assert.That(boleto2.ObtenerEsTrasbordo(), Is.True);
        Assert.That(boleto2.EsTransbordo, Is.True);
        Assert.That(boleto2.TotalAbonado, Is.EqualTo(0m));
        Assert.That(boleto2.MontoPagado, Is.EqualTo(0m));
        Assert.That(boleto2.Linea, Is.EqualTo("144 Negra"));
        Assert.That(tarjeta.Saldo, Is.EqualTo(2850m));
        Assert.That(boleto2.SaldoRestante, Is.EqualTo(2850m));
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
        Assert.That(b1, Is.Not.Null);
        Assert.That(b1!.EsTrasbordo, Is.False);
        Assert.That(b1.TotalAbonado, Is.EqualTo(2150m));

        var b2 = colectivoB.PagarCon(tarjeta, inicio.AddMinutes(15));
        Assert.That(b2, Is.Not.Null);
        Assert.That(b2!.EsTrasbordo, Is.True);
        Assert.That(b2.TotalAbonado, Is.EqualTo(0m));

        var b3 = colectivoC.PagarCon(tarjeta, inicio.AddMinutes(35));
        Assert.That(b3, Is.Not.Null);
        Assert.That(b3!.EsTrasbordo, Is.True);
        Assert.That(b3.TotalAbonado, Is.EqualTo(0m));

        var b4 = colectivoD.PagarCon(tarjeta, inicio.AddMinutes(55));
        Assert.That(b4, Is.Not.Null);
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
    public void Trasbordo_EnLimitesHorarios_0700Y2200_SonValidos()
    {
        var col1 = new Colectivo("102");
        var col2 = new Colectivo("144");
        var tarjeta = new Tarjeta(10000m);

        var fecha7amInicio = new DateTime(2026, 10, 19, 7, 0, 0);
        var fecha7amTrasbordo = new DateTime(2026, 10, 19, 7, 20, 0);

        col1.PagarCon(tarjeta, fecha7amInicio);
        var boletoManiana = col2.PagarCon(tarjeta, fecha7amTrasbordo);
        Assert.That(boletoManiana!.EsTrasbordo, Is.True);

        var fecha21pmInicio = new DateTime(2026, 10, 19, 21, 30, 0);
        var fecha22pmTrasbordo = new DateTime(2026, 10, 19, 22, 0, 0);

        col1.PagarCon(tarjeta, fecha21pmInicio);
        var boletoNoche = col2.PagarCon(tarjeta, fecha22pmTrasbordo);
        Assert.That(boletoNoche!.EsTrasbordo, Is.True);
    }

    [Test]
    public void Trasbordo_TodasLasTarjetasPuedenRealizarlo_MedioBoleto()
    {
        var col1 = new Colectivo("102");
        var col2 = new Colectivo("144");
        var col3 = new Colectivo("102");
        var tarjetaMedio = new Tarjeta(5000m, new TarjetaTipo(TarjetaTipo.NombreMedioBoleto, 0.5m));

        var maniana1 = new DateTime(2026, 10, 19, 8, 0, 0);
        var maniana2 = new DateTime(2026, 10, 19, 8, 20, 0);
        var mediodia = new DateTime(2026, 10, 19, 12, 0, 0);

        var b1 = col1.PagarCon(tarjetaMedio, maniana1);
        Assert.That(b1, Is.Not.Null);
        Assert.That(b1!.TotalAbonado, Is.EqualTo(1075m));

        var b2 = col2.PagarCon(tarjetaMedio, maniana2);
        Assert.That(b2, Is.Not.Null);
        Assert.That(b2!.EsTrasbordo, Is.True);
        Assert.That(b2.TotalAbonado, Is.EqualTo(0m));

        var b3 = col3.PagarCon(tarjetaMedio, mediodia);
        Assert.That(b3, Is.Not.Null);
        Assert.That(b3!.TotalAbonado, Is.EqualTo(1075m));
    }

    [Test]
    public void Trasbordo_TodasLasTarjetasPuedenRealizarlo_FranquiciaCompleta()
    {
        var col1 = new Colectivo("102");
        var col2 = new Colectivo("144");
        var col3 = new Colectivo("102");
        var tarjetaCompleta = new Tarjeta(0m, new TarjetaTipo(TarjetaTipo.NombreFranquiciaCompleta, 0m));

        var maniana1 = new DateTime(2026, 10, 19, 8, 0, 0);
        var maniana2 = new DateTime(2026, 10, 19, 8, 20, 0);
        var mediodia = new DateTime(2026, 10, 19, 12, 0, 0);

        var b1 = col1.PagarCon(tarjetaCompleta, maniana1);
        Assert.That(b1!.TotalAbonado, Is.EqualTo(0m));

        var b2 = col2.PagarCon(tarjetaCompleta, maniana2);
        Assert.That(b2!.EsTrasbordo, Is.True);
        Assert.That(b2.TotalAbonado, Is.EqualTo(0m));

        var b3 = col3.PagarCon(tarjetaCompleta, mediodia);
        Assert.That(b3!.TotalAbonado, Is.EqualTo(0m));
    }

    [Test]
    public void Trasbordo_EntreLineaUrbanaEInterurbana_EsGratuito()
    {
        var urbano = new Colectivo("102");
        var interurbano = new ColectivoInterurbano("Rosario - Gálvez");
        var tarjeta = new Tarjeta(10000m);

        var t1 = new DateTime(2026, 10, 19, 9, 0, 0);
        var t2 = new DateTime(2026, 10, 19, 9, 30, 0);

        var b1 = urbano.PagarCon(tarjeta, t1);
        Assert.That(b1!.TotalAbonado, Is.EqualTo(2150m));

        var b2 = interurbano.PagarCon(tarjeta, t2);
        Assert.That(b2!.EsTrasbordo, Is.True);
        Assert.That(b2.TotalAbonado, Is.EqualTo(0m));

        var tarjeta2 = new Tarjeta(10000m);
        var b3 = interurbano.PagarCon(tarjeta2, t1);
        Assert.That(b3!.TotalAbonado, Is.EqualTo(3000m));

        var b4 = urbano.PagarCon(tarjeta2, t2);
        Assert.That(b4!.EsTrasbordo, Is.True);
        Assert.That(b4.TotalAbonado, Is.EqualTo(0m));
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

        var b1 = col1.PagarCon(tarjeta, t1);
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
}
