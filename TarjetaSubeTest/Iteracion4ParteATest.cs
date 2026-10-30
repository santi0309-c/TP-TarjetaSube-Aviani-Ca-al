namespace TarjetaSubeTest;

using NUnit.Framework;
using TarjetaSube;

[TestFixture]
public class Iteracion4ParteATest
{
    [Test]
    public void TarifaBasica_AumentoCorrectamenteADosMilCientoCincuenta()
    {
        var colectivo = new Colectivo("144 Negra");

        Assert.That(Colectivo.TarifaBasica, Is.EqualTo(2150m));
        Assert.That(colectivo.Tarifa, Is.EqualTo(2150m));
    }

    [Test]
    public void TarjetaNormal_PagaTarifaActualizadaDe2150()
    {
        var colectivo = new Colectivo("102 Roja");
        var tarjeta = new Tarjeta(5000m);
        var fecha = new DateTime(2026, 10, 20, 10, 0, 0);

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.TotalAbonado, Is.EqualTo(2150m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m - 2150m));
    }

    [Test]
    public void UsoFrecuente_Viajes1Al29_PaganTarifaNormalSinDescuento()
    {
        var colectivo = new Colectivo("144");
        var tarjeta = new Tarjeta(40000m);
        var fechaBase = new DateTime(2026, 10, 1, 8, 0, 0);

        for (int i = 0; i < 28; i++)
        {
            tarjeta.RegistrarViaje(fechaBase.AddHours(i));
        }

        decimal tarifaViaje29 = tarjeta.CalcularPrecio(Colectivo.TarifaBasica, fechaBase);
        Assert.That(tarifaViaje29, Is.EqualTo(2150m), "El viaje 29 debe pagar tarifa normal completa ($2150)");
    }

    [Test]
    public void UsoFrecuente_Viaje30Al79_PaganCon20PorCientoDeDescuento()
    {
        var tarjeta = new Tarjeta(40000m);
        var fechaBase = new DateTime(2026, 10, 1, 8, 0, 0);

        for (int i = 0; i < 29; i++)
        {
            tarjeta.RegistrarViaje(fechaBase.AddHours(i));
        }

        decimal tarifaEsperada20Desc = 2150m * 0.80m;

        decimal tarifaViaje30 = tarjeta.CalcularPrecio(Colectivo.TarifaBasica, fechaBase);
        Assert.That(tarifaViaje30, Is.EqualTo(tarifaEsperada20Desc), "El viaje 30 debe tener 20% de descuento ($1720)");

        for (int i = 29; i < 78; i++)
        {
            tarjeta.RegistrarViaje(fechaBase.AddHours(i));
        }

        decimal tarifaViaje79 = tarjeta.CalcularPrecio(Colectivo.TarifaBasica, fechaBase);
        Assert.That(tarifaViaje79, Is.EqualTo(tarifaEsperada20Desc), "El viaje 79 debe tener 20% de descuento ($1720)");
    }

    [Test]
    public void UsoFrecuente_Viaje80EnAdelante_PaganCon25PorCientoDeDescuento()
    {
        var tarjeta = new Tarjeta(40000m);
        var fechaBase = new DateTime(2026, 10, 1, 8, 0, 0);

        for (int i = 0; i < 79; i++)
        {
            tarjeta.RegistrarViaje(fechaBase.AddHours(i));
        }

        decimal tarifaEsperada25Desc = 2150m * 0.75m;

        decimal tarifaViaje80 = tarjeta.CalcularPrecio(Colectivo.TarifaBasica, fechaBase);
        Assert.That(tarifaViaje80, Is.EqualTo(tarifaEsperada25Desc), "El viaje 80 debe tener 25% de descuento ($1612.50)");

        tarjeta.RegistrarViaje(fechaBase.AddHours(79));
        decimal tarifaViaje81 = tarjeta.CalcularPrecio(Colectivo.TarifaBasica, fechaBase);
        Assert.That(tarifaViaje81, Is.EqualTo(tarifaEsperada25Desc), "El viaje 81 en adelante también tiene 25% de descuento");
    }

    [Test]
    public void UsoFrecuente_AlCambiarDeMes_SeReiniciaElBeneficio()
    {
        var tarjeta = new Tarjeta(40000m);
        var fechaOctubre = new DateTime(2026, 10, 20, 8, 0, 0);
        var fechaNoviembre = new DateTime(2026, 11, 1, 8, 0, 0);

        for (int i = 0; i < 50; i++)
        {
            tarjeta.RegistrarViaje(fechaOctubre.AddMinutes(i * 10));
        }

        decimal tarifaOctubre = tarjeta.CalcularPrecio(Colectivo.TarifaBasica, fechaOctubre);
        Assert.That(tarifaOctubre, Is.EqualTo(2150m * 0.80m));

        decimal tarifaNoviembre = tarjeta.CalcularPrecio(Colectivo.TarifaBasica, fechaNoviembre);
        Assert.That(tarifaNoviembre, Is.EqualTo(2150m), "Al cambiar de mes, el contador mensual se reinicia");
    }

    [Test]
    public void UsoFrecuente_NoAplicaAFranquicias_AplicaSoloATarjetaNormal()
    {
        var tipoMedio = new TarjetaTipo(TarjetaTipo.NombreMedioBoleto, 0.5m);
        var tarjetaMedio = new Tarjeta(40000m, tipoMedio);
        var fecha = new DateTime(2026, 10, 20, 10, 0, 0);

        for (int i = 1; i <= 25; i++)
        {
            tarjetaMedio.RegistrarViaje(new DateTime(2026, 10, 1, 8, 0, 0).AddDays(i % 10).AddMinutes(i));
        }

        decimal tarifa = tarjetaMedio.CalcularPrecio(Colectivo.TarifaBasica, fecha);
        Assert.That(tarifa, Is.EqualTo(2150m / 2m), "Medio Boleto no debe utilizar la escala de uso frecuente");
    }

    [Test]
    public void Franquicias_ViajarEnHorarioPermitido_LunesAViernesDe6A22_PermiteElViaje()
    {
        var colectivo = new Colectivo("144 Negra");
        var tipo = new TarjetaTipo(TarjetaTipo.NombreMedioBoleto, 0.5m);
        var tarjeta = new Tarjeta(5000m, tipo);

        var lunes6am = new DateTime(2026, 10, 19, 6, 0, 0);
        Assert.That(colectivo.PagarCon(tarjeta, lunes6am), Is.Not.Null, "Lunes a las 06:00 debe permitir viajar");

        var miercolesTarde = new DateTime(2026, 10, 21, 14, 30, 0);
        Assert.That(colectivo.PagarCon(tarjeta, miercolesTarde), Is.Not.Null, "Miércoles a las 14:30 debe permitir viajar");

        var viernes22pm = new DateTime(2026, 10, 23, 22, 0, 0);
        Assert.That(colectivo.PagarCon(tarjeta, viernes22pm), Is.Not.Null, "Viernes a las 22:00 debe permitir viajar");
    }

    [Test]
    public void Franquicias_ViajarAntesDeLas6_RechazaElViajeYRetornaNull()
    {
        var colectivo = new Colectivo("102");
        var tipo = new TarjetaTipo(TarjetaTipo.NombreMedioBoleto, 0.5m);
        var tarjeta = new Tarjeta(5000m, tipo);

        var lunesMadrugada = new DateTime(2026, 10, 19, 5, 59, 59);

        var boleto = colectivo.PagarCon(tarjeta, lunesMadrugada);

        Assert.That(boleto, Is.Null, "No se puede viajar antes de las 06:00 con franquicia");
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m), "El saldo no debe descontarse si el viaje fue rechazado");
    }

    [Test]
    public void Franquicias_ViajarDespuesDeLas22_RechazaElViajeYRetornaNull()
    {
        var colectivo = new Colectivo("102");
        var tipo = new TarjetaTipo(TarjetaTipo.NombreFranquiciaCompleta, 0m);
        var tarjeta = new Tarjeta(5000m, tipo);

        var martesNoche = new DateTime(2026, 10, 20, 22, 0, 1);

        var boleto = colectivo.PagarCon(tarjeta, martesNoche);

        Assert.That(boleto, Is.Null, "No se puede viajar después de las 22:00 con franquicia");
    }

    [Test]
    public void Franquicias_ViajarEnFinDeSemana_SabadoODomingo_RechazaElViaje()
    {
        var colectivo = new Colectivo("115");
        var tipo = new TarjetaTipo(TarjetaTipo.NombreMedioBoleto, 0.5m);
        var tarjeta = new Tarjeta(5000m, tipo);

        var sabado = new DateTime(2026, 10, 24, 12, 0, 0);
        Assert.That(sabado.DayOfWeek, Is.EqualTo(DayOfWeek.Saturday));
        var boletoSabado = colectivo.PagarCon(tarjeta, sabado);
        Assert.That(boletoSabado, Is.Null, "Las franquicias no pueden utilizarse los sábados");

        var domingo = new DateTime(2026, 10, 25, 15, 0, 0);
        Assert.That(domingo.DayOfWeek, Is.EqualTo(DayOfWeek.Sunday));
        var boletoDomingo = colectivo.PagarCon(tarjeta, domingo);
        Assert.That(boletoDomingo, Is.Null, "Las franquicias no pueden utilizarse los domingos");
    }

    [Test]
    public void TarjetaNormal_PuedeViajarCualquierDiaYHorario_SinRestriccion()
    {
        var colectivo = new Colectivo("K");
        var tarjeta = new Tarjeta(10000m);

        var sabadoNoche = new DateTime(2026, 10, 24, 23, 30, 0);
        var boletoSabado = colectivo.PagarCon(tarjeta, sabadoNoche);
        Assert.That(boletoSabado, Is.Not.Null, "La tarjeta normal puede viajar un sábado a la noche");

        var domingoMadrugada = new DateTime(2026, 10, 25, 3, 0, 0);
        var boletoDomingo = colectivo.PagarCon(tarjeta, domingoMadrugada);
        Assert.That(boletoDomingo, Is.Not.Null, "La tarjeta normal puede viajar un domingo de madrugada");
    }

    [Test]
    public void TodasLasFranquicias_RespetanLaFranjaHoraria()
    {
        var colectivo = new Colectivo("144");
        var domingo = new DateTime(2026, 10, 25, 10, 0, 0);

        var tarjetaMedio = new Tarjeta(5000m, new TarjetaTipo(TarjetaTipo.NombreMedioBoleto, 0.5m));
        var tarjetaCompleta = new Tarjeta(5000m, new TarjetaTipo(TarjetaTipo.NombreFranquiciaCompleta, 0m));
        var tarjetaGratuito = new Tarjeta(5000m, new TarjetaTipo(TarjetaTipo.NombreBoletoGratuito, 0m));
        var tarjetaSocial = new Tarjeta(5000m, new TarjetaTipo("Tarifa Social", 0.45m));

        Assert.That(colectivo.PagarCon(tarjetaMedio, domingo), Is.Null, "Medio Boleto debe ser rechazado en domingo");
        Assert.That(colectivo.PagarCon(tarjetaCompleta, domingo), Is.Null, "Franquicia Completa debe ser rechazada en domingo");
        Assert.That(colectivo.PagarCon(tarjetaGratuito, domingo), Is.Null, "Boleto Gratuito debe ser rechazado en domingo");
        Assert.That(colectivo.PagarCon(tarjetaSocial, domingo), Is.Null, "Tarifa Social debe ser rechazada en domingo");
    }
}
