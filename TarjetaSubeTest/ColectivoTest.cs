namespace TarjetaSubeTest;

using NUnit.Framework;
using TarjetaSube;

[TestFixture]
public class ColectivoTest
{
    private static readonly DateTime FechaHabilValida = new(2026, 10, 19, 10, 0, 0); // Lunes 10:00

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
    public void Franquicia_DiaInhabil_RechazaPagoRetornandoNull()
    {
        var domingo = new DateTime(2026, 10, 25, 14, 0, 0); // Domingo
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

        // Antes de las 06:00
        var temprano = new DateTime(2026, 10, 19, 5, 59, 59);
        var boletoTemprano = colectivo.PagarCon(tarjeta, temprano);
        Assert.That(boletoTemprano, Is.Null);

        // Después de las 22:00
        var tarde = new DateTime(2026, 10, 19, 22, 0, 1);
        var boletoTarde = colectivo.PagarCon(tarjeta, tarde);
        Assert.That(boletoTarde, Is.Null);

        // En horario permitido (06:00 exacto)
        var enHorario = new DateTime(2026, 10, 19, 6, 0, 0);
        var boletoEnHorario = colectivo.PagarCon(tarjeta, enHorario);
        Assert.That(boletoEnHorario, Is.Not.Null);
    }

    [Test]
    public void TarjetaNormal_SinRestriccionHorariaNiDeDia_ViajaEnCualquierMomento()
    {
        var colectivo = new Colectivo("102");
        var tarjeta = new Tarjeta(10000m);

        // Domingo a la madrugada
        var domingoMadrugada = new DateTime(2026, 10, 25, 3, 30, 0);
        var boleto = colectivo.PagarCon(tarjeta, domingoMadrugada);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(10000m - 2150m));
    }
}
