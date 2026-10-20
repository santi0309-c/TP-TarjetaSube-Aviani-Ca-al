namespace TarjetaSubeTest;

using NUnit.Framework;
using TarjetaSube;

[TestFixture]
public class Iteracion3ParteATest
{
    // ═════════════════════════════════════════════════════════════════════════
    // 1. MÁS DATOS SOBRE EL BOLETO
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public void Boleto_PermiteConocerTodosLosDatosRequeridos_MediantePropiedadesYMetodos()
    {
        var tipo = new TarjetaTipo(TarjetaTipo.NombreNormal, 1m);
        var tarjeta = new Tarjeta(5000m, tipo) { Id = 42 };
        var colectivo = new Colectivo("144 Negra");
        var fecha = new DateTime(2026, 10, 20, 14, 30, 0);

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Not.Null);

        // Fecha
        Assert.That(boleto!.Fecha, Is.EqualTo(fecha));
        Assert.That(boleto.ObtenerFecha(), Is.EqualTo(fecha));

        // Tipo de tarjeta
        Assert.That(boleto.TipoTarjeta, Is.EqualTo(TarjetaTipo.NombreNormal));
        Assert.That(boleto.ObtenerTipoTarjeta(), Is.EqualTo(TarjetaTipo.NombreNormal));

        // Línea de colectivo
        Assert.That(boleto.Linea, Is.EqualTo("144 Negra"));
        Assert.That(boleto.ObtenerLinea(), Is.EqualTo("144 Negra"));

        // Total abonado
        Assert.That(boleto.TotalAbonado, Is.EqualTo(Colectivo.TarifaBasica));
        Assert.That(boleto.MontoPagado, Is.EqualTo(Colectivo.TarifaBasica));
        Assert.That(boleto.ObtenerTotalAbonado(), Is.EqualTo(Colectivo.TarifaBasica));

        // Saldo e indicación de saldo restante
        decimal saldoEsperado = 5000m - Colectivo.TarifaBasica;
        Assert.That(boleto.Saldo, Is.EqualTo(saldoEsperado));
        Assert.That(boleto.SaldoRestante, Is.EqualTo(saldoEsperado));
        Assert.That(boleto.ObtenerSaldo(), Is.EqualTo(saldoEsperado));

        // ID de la tarjeta
        Assert.That(boleto.TarjetaId, Is.EqualTo(42));
        Assert.That(boleto.ObtenerTarjetaId(), Is.EqualTo(42));
    }

    [Test]
    public void Boleto_PosiblesTiposDeTarjeta_GeneranBoletosCorrectos()
    {
        var colectivo = new Colectivo("102 Roja");
        var fecha = new DateTime(2026, 10, 20, 10, 0, 0);

        // 1. Tarjeta Normal
        var tarjetaNormal = new Tarjeta(5000m, new TarjetaTipo(TarjetaTipo.NombreNormal, 1m)) { Id = 1 };
        var boletoNormal = colectivo.PagarCon(tarjetaNormal, fecha);
        Assert.That(boletoNormal!.ObtenerTipoTarjeta(), Is.EqualTo(TarjetaTipo.NombreNormal));
        Assert.That(boletoNormal.ObtenerTotalAbonado(), Is.EqualTo(Colectivo.TarifaBasica));

        // 2. Medio Boleto Estudiantil
        var tarjetaMedio = new Tarjeta(5000m, new TarjetaTipo(TarjetaTipo.NombreMedioBoleto, 0.5m)) { Id = 2 };
        var boletoMedio = colectivo.PagarCon(tarjetaMedio, fecha);
        Assert.That(boletoMedio!.ObtenerTipoTarjeta(), Is.EqualTo(TarjetaTipo.NombreMedioBoleto));
        Assert.That(boletoMedio.ObtenerTotalAbonado(), Is.EqualTo(Colectivo.TarifaBasica / 2m));

        // 3. Franquicia Completa
        var tarjetaFranquicia = new Tarjeta(5000m, new TarjetaTipo(TarjetaTipo.NombreFranquiciaCompleta, 0m)) { Id = 3 };
        var boletoFranquicia = colectivo.PagarCon(tarjetaFranquicia, fecha);
        Assert.That(boletoFranquicia!.ObtenerTipoTarjeta(), Is.EqualTo(TarjetaTipo.NombreFranquiciaCompleta));
        Assert.That(boletoFranquicia.ObtenerTotalAbonado(), Is.EqualTo(0m));

        // 4. Boleto Gratuito Estudiantil
        var tarjetaGratuito = new Tarjeta(5000m, new TarjetaTipo(TarjetaTipo.NombreBoletoGratuito, 0m)) { Id = 4 };
        var boletoGratuito = colectivo.PagarCon(tarjetaGratuito, fecha);
        Assert.That(boletoGratuito!.ObtenerTipoTarjeta(), Is.EqualTo(TarjetaTipo.NombreBoletoGratuito));
        Assert.That(boletoGratuito.ObtenerTotalAbonado(), Is.EqualTo(0m));
    }

    [Test]
    public void Boleto_ConSaldoNegativoPrevio_InformaTotalAbonadoSuperiorATarifaNormal()
    {
        // Tarjeta con saldo inicial $1380.
        // Al primer viaje de $1580, el saldo queda en -$200 (saldo negativo).
        var tarjeta = new Tarjeta(1380m) { Id = 99 };
        var colectivo = new Colectivo("115");
        var fecha1 = new DateTime(2026, 10, 20, 8, 0, 0);
        var fecha2 = new DateTime(2026, 10, 20, 12, 0, 0);

        var primerBoleto = colectivo.PagarCon(tarjeta, fecha1);
        Assert.That(primerBoleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-200m));
        Assert.That(primerBoleto!.TotalAbonado, Is.EqualTo(Colectivo.TarifaBasica));

        // Segundo viaje: la tarjeta ya tenía saldo negativo (-$200).
        // El viaje cuesta $1580, quedando el nuevo saldo en -$1780 (dentro del límite de -$2000).
        // El boleto deberá informar el monto total abonado: tarifa ($1580) + deuda previa ($200) = $1780.
        var segundoBoleto = colectivo.PagarCon(tarjeta, fecha2);

        Assert.That(segundoBoleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-1780m));
        Assert.That(segundoBoleto!.TotalAbonado, Is.EqualTo(1780m));
        Assert.That(segundoBoleto.TotalAbonado, Is.GreaterThan(Colectivo.TarifaBasica),
            "El total abonado debe ser superior a la tarifa normal por incluir el saldo negativo anterior");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // 2. LIMITACIÓN EN EL PAGO DE MEDIO BOLETO: 5 MINUTOS
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public void MedioBoleto_ViajarEnMenosDe5Minutos_NoPermiteElViajeYRetornaNull()
    {
        var tipo = new TarjetaTipo(TarjetaTipo.NombreMedioBoleto, 0.5m);
        var tarjeta = new Tarjeta(5000m, tipo);
        var colectivo = new Colectivo("102 Roja");

        var fecha1 = new DateTime(2026, 10, 20, 10, 0, 0);
        var fecha2 = fecha1.AddMinutes(4).AddSeconds(59); // 4 minutos y 59 segundos

        var boleto1 = colectivo.PagarCon(tarjeta, fecha1);
        decimal saldoDespuesPrimerViaje = tarjeta.Saldo;

        var boleto2 = colectivo.PagarCon(tarjeta, fecha2);

        Assert.That(boleto1, Is.Not.Null, "El primer viaje debe permitirse");
        Assert.That(boleto2, Is.Null, "No debe permitir marcar nuevamente antes de los 5 minutos con MedioBoleto");
        Assert.That(tarjeta.Saldo, Is.EqualTo(saldoDespuesPrimerViaje), "El saldo no debe modificarse si el viaje fue rechazado");
    }

    [Test]
    public void MedioBoleto_ViajarDespuesDe5Minutos_PermiteElViaje()
    {
        var tipo = new TarjetaTipo(TarjetaTipo.NombreMedioBoleto, 0.5m);
        var tarjeta = new Tarjeta(5000m, tipo);
        var colectivo = new Colectivo("102 Roja");

        var fecha1 = new DateTime(2026, 10, 20, 10, 0, 0);
        var fecha2 = fecha1.AddMinutes(5); // Exactamente 5 minutos después

        var boleto1 = colectivo.PagarCon(tarjeta, fecha1);
        var boleto2 = colectivo.PagarCon(tarjeta, fecha2);

        Assert.That(boleto1, Is.Not.Null);
        Assert.That(boleto2, Is.Not.Null, "Debe permitir el viaje si pasaron 5 minutos o más");
    }

    [Test]
    public void TarjetaNormal_NoTieneRestriccionDeCincoMinutos()
    {
        var tipo = new TarjetaTipo(TarjetaTipo.NombreNormal, 1m);
        var tarjeta = new Tarjeta(5000m, tipo);
        var colectivo = new Colectivo("102 Roja");

        var fecha1 = new DateTime(2026, 10, 20, 10, 0, 0);
        var fecha2 = fecha1.AddMinutes(1); // 1 minuto después

        var boleto1 = colectivo.PagarCon(tarjeta, fecha1);
        var boleto2 = colectivo.PagarCon(tarjeta, fecha2);

        Assert.That(boleto1, Is.Not.Null);
        Assert.That(boleto2, Is.Not.Null, "Tarjeta Normal no debe tener limitación de 5 minutos");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // 3. MEDIO BOLETO: MÁXIMO 2 VIAJES CON DESCUENTO POR DÍA
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public void MedioBoleto_PrimerosDosViajesMismoDia_PaganLaMitadDeTarifaNormal()
    {
        var tipo = new TarjetaTipo(TarjetaTipo.NombreMedioBoleto, 0.5m);
        var tarjeta = new Tarjeta(5000m, tipo);
        var colectivo = new Colectivo("102 Roja");

        var fecha1 = new DateTime(2026, 10, 20, 8, 0, 0);
        var fecha2 = new DateTime(2026, 10, 20, 12, 0, 0);

        var boleto1 = colectivo.PagarCon(tarjeta, fecha1);
        var boleto2 = colectivo.PagarCon(tarjeta, fecha2);

        decimal tarifaMedio = Colectivo.TarifaBasica / 2m; // $790
        Assert.That(boleto1!.TotalAbonado, Is.EqualTo(tarifaMedio), "Primer viaje paga la mitad");
        Assert.That(boleto2!.TotalAbonado, Is.EqualTo(tarifaMedio), "Segundo viaje paga la mitad");
    }

    [Test]
    public void MedioBoleto_TercerViajeMismoDia_SeCobraATarifaCompleta()
    {
        var tipo = new TarjetaTipo(TarjetaTipo.NombreMedioBoleto, 0.5m);
        var tarjeta = new Tarjeta(10000m, tipo);
        var colectivo = new Colectivo("102 Roja");

        var fecha1 = new DateTime(2026, 10, 20, 8, 0, 0);
        var fecha2 = new DateTime(2026, 10, 20, 10, 0, 0);
        var fecha3 = new DateTime(2026, 10, 20, 14, 0, 0);

        var boleto1 = colectivo.PagarCon(tarjeta, fecha1);
        var boleto2 = colectivo.PagarCon(tarjeta, fecha2);
        var boleto3 = colectivo.PagarCon(tarjeta, fecha3);

        decimal tarifaMedio = Colectivo.TarifaBasica / 2m;
        Assert.That(boleto1!.TotalAbonado, Is.EqualTo(tarifaMedio), "1er viaje: medio boleto");
        Assert.That(boleto2!.TotalAbonado, Is.EqualTo(tarifaMedio), "2do viaje: medio boleto");
        Assert.That(boleto3!.TotalAbonado, Is.EqualTo(Colectivo.TarifaBasica), "3er viaje: tarifa completa");
    }

    [Test]
    public void MedioBoleto_CuartoViajeMismoDia_TambienSeCobraATarifaCompleta()
    {
        var tipo = new TarjetaTipo(TarjetaTipo.NombreMedioBoleto, 0.5m);
        var tarjeta = new Tarjeta(10000m, tipo);
        var colectivo = new Colectivo("102 Roja");

        var fecha1 = new DateTime(2026, 10, 20, 8, 0, 0);
        var fecha2 = new DateTime(2026, 10, 20, 9, 0, 0);
        var fecha3 = new DateTime(2026, 10, 20, 10, 0, 0);
        var fecha4 = new DateTime(2026, 10, 20, 11, 0, 0);

        colectivo.PagarCon(tarjeta, fecha1);
        colectivo.PagarCon(tarjeta, fecha2);
        var boleto3 = colectivo.PagarCon(tarjeta, fecha3);
        var boleto4 = colectivo.PagarCon(tarjeta, fecha4);

        Assert.That(boleto3!.TotalAbonado, Is.EqualTo(Colectivo.TarifaBasica));
        Assert.That(boleto4!.TotalAbonado, Is.EqualTo(Colectivo.TarifaBasica));
    }

    [Test]
    public void MedioBoleto_ViajesConDescuento_SeReinicianAlDiaSiguiente()
    {
        var tipo = new TarjetaTipo(TarjetaTipo.NombreMedioBoleto, 0.5m);
        var tarjeta = new Tarjeta(10000m, tipo);
        var colectivo = new Colectivo("102 Roja");

        var hoy = new DateTime(2026, 10, 20, 8, 0, 0);
        var mañana = new DateTime(2026, 10, 21, 8, 0, 0);

        // Hoy: 3 viajes (dos con descuento, uno tarifa completa)
        colectivo.PagarCon(tarjeta, hoy);
        colectivo.PagarCon(tarjeta, hoy.AddMinutes(10));
        var tercerViajeHoy = colectivo.PagarCon(tarjeta, hoy.AddMinutes(20));
        Assert.That(tercerViajeHoy!.TotalAbonado, Is.EqualTo(Colectivo.TarifaBasica));

        // Mañana: el primer viaje debe volver a tener descuento de medio boleto
        var primerViajeMañana = colectivo.PagarCon(tarjeta, mañana);
        decimal tarifaMedio = Colectivo.TarifaBasica / 2m;
        Assert.That(primerViajeMañana!.TotalAbonado, Is.EqualTo(tarifaMedio),
            "Al día siguiente deben reiniciarse los viajes con medio boleto");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // 4. FRANQUICIA COMPLETA: MÁXIMO 2 VIAJES GRATUITOS POR DÍA
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public void FranquiciaCompleta_PrimerosDosViajesMismoDia_SonGratuitos()
    {
        var tipo = new TarjetaTipo(TarjetaTipo.NombreFranquiciaCompleta, 0m);
        var tarjeta = new Tarjeta(5000m, tipo);
        var colectivo = new Colectivo("144 Negra");

        var fecha1 = new DateTime(2026, 10, 20, 8, 0, 0);
        var fecha2 = new DateTime(2026, 10, 20, 12, 0, 0);

        var boleto1 = colectivo.PagarCon(tarjeta, fecha1);
        var boleto2 = colectivo.PagarCon(tarjeta, fecha2);

        Assert.That(boleto1!.TotalAbonado, Is.EqualTo(0m), "1er viaje: gratuito");
        Assert.That(boleto2!.TotalAbonado, Is.EqualTo(0m), "2do viaje: gratuito");
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m), "Los dos primeros viajes no descuentan saldo");
    }

    [Test]
    public void FranquiciaCompleta_TercerViajeMismoDia_SeCobraATarifaCompleta()
    {
        var tipo = new TarjetaTipo(TarjetaTipo.NombreFranquiciaCompleta, 0m);
        var tarjeta = new Tarjeta(5000m, tipo);
        var colectivo = new Colectivo("K");

        var fecha1 = new DateTime(2026, 10, 20, 8, 0, 0);
        var fecha2 = new DateTime(2026, 10, 20, 10, 0, 0);
        var fecha3 = new DateTime(2026, 10, 20, 14, 0, 0);

        var boleto1 = colectivo.PagarCon(tarjeta, fecha1);
        var boleto2 = colectivo.PagarCon(tarjeta, fecha2);
        var boleto3 = colectivo.PagarCon(tarjeta, fecha3);

        Assert.That(boleto1!.TotalAbonado, Is.EqualTo(0m), "1er viaje: gratuito");
        Assert.That(boleto2!.TotalAbonado, Is.EqualTo(0m), "2do viaje: gratuito");
        Assert.That(boleto3!.TotalAbonado, Is.EqualTo(Colectivo.TarifaBasica), "3er viaje: tarifa completa");
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m - Colectivo.TarifaBasica));
    }

    [Test]
    public void FranquiciaCompleta_ViajesPosterioresAlSegundo_PaganPrecioCompleto()
    {
        var tipo = new TarjetaTipo(TarjetaTipo.NombreFranquiciaCompleta, 0m);
        var tarjeta = new Tarjeta(10000m, tipo);
        var colectivo = new Colectivo("115");

        var fecha1 = new DateTime(2026, 10, 20, 8, 0, 0);
        var fecha2 = new DateTime(2026, 10, 20, 9, 0, 0);
        var fecha3 = new DateTime(2026, 10, 20, 10, 0, 0);
        var fecha4 = new DateTime(2026, 10, 20, 11, 0, 0);

        colectivo.PagarCon(tarjeta, fecha1); // gratuito
        colectivo.PagarCon(tarjeta, fecha2); // gratuito
        var boleto3 = colectivo.PagarCon(tarjeta, fecha3);
        var boleto4 = colectivo.PagarCon(tarjeta, fecha4);

        Assert.That(boleto3!.TotalAbonado, Is.EqualTo(Colectivo.TarifaBasica));
        Assert.That(boleto4!.TotalAbonado, Is.EqualTo(Colectivo.TarifaBasica));
    }

    [Test]
    public void FranquiciaCompleta_ViajesGratuitos_SeReinicianAlDiaSiguiente()
    {
        var tipo = new TarjetaTipo(TarjetaTipo.NombreFranquiciaCompleta, 0m);
        var tarjeta = new Tarjeta(5000m, tipo);
        var colectivo = new Colectivo("K");

        var hoy = new DateTime(2026, 10, 20, 8, 0, 0);
        var mañana = new DateTime(2026, 10, 21, 8, 0, 0);

        // Agotar viajes gratuitos hoy
        colectivo.PagarCon(tarjeta, hoy);
        colectivo.PagarCon(tarjeta, hoy.AddMinutes(10));
        var tercerViajeHoy = colectivo.PagarCon(tarjeta, hoy.AddMinutes(20));
        Assert.That(tercerViajeHoy!.TotalAbonado, Is.EqualTo(Colectivo.TarifaBasica));

        // Mañana el viaje vuelve a ser gratuito
        var primerViajeMañana = colectivo.PagarCon(tarjeta, mañana);
        Assert.That(primerViajeMañana!.TotalAbonado, Is.EqualTo(0m),
            "Al día siguiente los viajes gratuitos deben reiniciarse");
    }

    [Test]
    public void BoletoGratuitoEstudiantil_TieneMismoComportamientoQueFranquiciaCompleta()
    {
        var tipo = new TarjetaTipo(TarjetaTipo.NombreBoletoGratuito, 0m);
        var tarjeta = new Tarjeta(5000m, tipo);
        var colectivo = new Colectivo("144 Negra");

        var fecha1 = new DateTime(2026, 10, 20, 8, 0, 0);
        var fecha2 = new DateTime(2026, 10, 20, 10, 0, 0);
        var fecha3 = new DateTime(2026, 10, 20, 14, 0, 0);

        var boleto1 = colectivo.PagarCon(tarjeta, fecha1);
        var boleto2 = colectivo.PagarCon(tarjeta, fecha2);
        var boleto3 = colectivo.PagarCon(tarjeta, fecha3);

        Assert.That(boleto1!.TotalAbonado, Is.EqualTo(0m), "1er viaje: gratuito");
        Assert.That(boleto2!.TotalAbonado, Is.EqualTo(0m), "2do viaje: gratuito");
        Assert.That(boleto3!.TotalAbonado, Is.EqualTo(Colectivo.TarifaBasica), "3er viaje: tarifa completa");
    }
}
