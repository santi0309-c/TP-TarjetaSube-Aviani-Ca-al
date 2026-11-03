namespace TarjetaSubeTest;

using NUnit.Framework;
using TarjetaSube;

[TestFixture]
public class TarjetaTest
{
    private static readonly DateTime FechaBaseLunes = new(2026, 10, 19, 10, 0, 0);

    [TestCase(2000)]
    [TestCase(3000)]
    [TestCase(4000)]
    [TestCase(5000)]
    [TestCase(8000)]
    [TestCase(10000)]
    [TestCase(15000)]
    [TestCase(20000)]
    [TestCase(25000)]
    [TestCase(30000)]
    public void CargarSaldo_ConMontosAceptados_AcreditaSaldoCorrectamente(decimal monto)
    {
        var tarjeta = new Tarjeta();

        bool resultado = tarjeta.CargarSaldo(monto);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(monto));
    }

    [TestCase(1000)]
    [TestCase(6000)]
    [TestCase(50000)]
    [TestCase(-2000)]
    public void CargarSaldo_ConMontosNoAceptados_RetornaFalseYNoModificaSaldo(decimal monto)
    {
        var tarjeta = new Tarjeta();

        bool resultado = tarjeta.CargarSaldo(monto);

        Assert.That(resultado, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(0));
    }

    [Test]
    public void LimiteSaldo_EsCincuentaYSeisMilPesos()
    {
        Assert.That(Tarjeta.LimiteSaldo, Is.EqualTo(56000m));
    }

    [Test]
    public void Constructor_ConSaldoInicialHasta56000_SeCreaCorrectamente()
    {
        var tarjeta = new Tarjeta(56000m);

        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(0m));
    }

    [Test]
    public void CargarSaldo_SuperandoLimiteMaximo_AcreditaHastaMaximoYDejaExcedenteEnSaldoPendiente()
    {
        var tarjeta = new Tarjeta(50000m);

        bool resultado = tarjeta.CargarSaldo(10000m);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(Tarjeta.LimiteSaldo));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(4000m));
    }

    [Test]
    public void CargarSaldo_CuandoYaEstaEnLimiteMaximo_GuardaMontoCompletoEnSaldoPendiente()
    {
        var tarjeta = new Tarjeta(56000m);

        bool resultado = tarjeta.CargarSaldo(5000m);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(5000m));
    }

    [Test]
    public void CargasMultiples_AcumulanSaldoPendienteCorrectamente()
    {
        var tarjeta = new Tarjeta(50000m);

        tarjeta.CargarSaldo(10000m);
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(4000m));

        tarjeta.CargarSaldo(8000m);
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(12000m));
    }

    [Test]
    public void DescontarSaldo_ConSaldoSuficiente_DescuentaYRetornaTrue()
    {
        var tarjeta = new Tarjeta(5000m);

        bool resultado = tarjeta.DescontarSaldo(1580m);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(3420m));
    }

    [Test]
    public void DescontarSaldo_ConSaldoInsuficiente_RetornaFalseYNoModificaSaldo()
    {
        var tarjeta = new Tarjeta();
        tarjeta.DescontarSaldo(1580m);

        bool resultado = tarjeta.DescontarSaldo(1580m);

        Assert.That(resultado, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-1580m));
    }

    [Test]
    public void DescontarSaldo_ConSaldoInsuficiente_PermiteViajePlusYSaldoQuedaNegativo()
    {
        var tarjeta = new Tarjeta(1000m);

        bool resultado = tarjeta.DescontarSaldo(1580m);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-580m));
        Assert.That(tarjeta.Saldo, Is.GreaterThanOrEqualTo(Tarjeta.LimiteNegativo));
    }

    [Test]
    public void DescontarSaldo_SuperandoLimiteNegativo_RetornaFalseYNoModificaSaldo()
    {
        var tarjeta = new Tarjeta();
        tarjeta.DescontarSaldo(1580m);

        bool resultado = tarjeta.DescontarSaldo(1580m);

        Assert.That(resultado, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-1580m));
    }

    [Test]
    public void DescontarSaldo_HastaLimiteNegativoExacto_RetornaTrue()
    {
        var tarjeta = new Tarjeta(0m);

        bool resultado = tarjeta.DescontarSaldo(2000m);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-2000m));
    }

    [Test]
    public void DescontarSaldo_SuperandoPorUnCentavoLimiteNegativo_RetornaFalse()
    {
        var tarjeta = new Tarjeta(0m);

        bool resultado = tarjeta.DescontarSaldo(2000.01m);

        Assert.That(resultado, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(0m));
    }

    [Test]
    public void CargarSaldo_ConDeudaNegativa_DescuentaDeudaYActualizaSaldoCorrectamente()
    {
        var tarjeta = new Tarjeta();
        tarjeta.DescontarSaldo(1580m);

        bool resultado = tarjeta.CargarSaldo(2000m);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(420m));
    }

    [Test]
    public void DescontarSaldo_ViajesPlus_SeIncrementaConCadaViajePlusOtorgado()
    {
        var tarjeta = new Tarjeta(1000m);
        Assert.That(tarjeta.ViajesPlus, Is.EqualTo(0));

        bool primerViaje = tarjeta.DescontarSaldo(1580m);
        Assert.That(primerViaje, Is.True);
        Assert.That(tarjeta.ViajesPlus, Is.EqualTo(1));

        bool segundoViaje = tarjeta.DescontarSaldo(500m);
        Assert.That(segundoViaje, Is.True);
        Assert.That(tarjeta.ViajesPlus, Is.EqualTo(2));
    }

    [Test]
    public void DescontarSaldo_ConSaldoSuficiente_NoIncrementaViajesPlus()
    {
        var tarjeta = new Tarjeta(5000m);

        tarjeta.DescontarSaldo(1580m);

        Assert.That(tarjeta.ViajesPlus, Is.EqualTo(0));
    }

    [Test]
    public void DescontarSaldo_ConSaldoExacto_DescuentaYRetornaTrueYSaldoQuedaEnCero()
    {
        var tarjeta = new Tarjeta(1580m);

        bool resultado = tarjeta.DescontarSaldo(1580m);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(0m));
    }

    [Test]
    public void DescontarSaldo_ConMontoCero_RetornaTrueYNoModificaSaldo()
    {
        var tarjeta = new Tarjeta(1000m);

        bool resultado = tarjeta.DescontarSaldo(0);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(1000m));
    }

    [Test]
    public void DescontarSaldo_ConMontoNegativo_RetornaFalseYNoModificaSaldo()
    {
        var tarjeta = new Tarjeta(1000m);

        bool resultado = tarjeta.DescontarSaldo(-100m);

        Assert.That(resultado, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(1000m));
    }

    [Test]
    public void Constructor_ConSaldoNegativo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Tarjeta(-500m));
    }

    [Test]
    public void Constructor_ConSaldoSuperiorAlLimite_LanzaExcepcion()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Tarjeta(Tarjeta.LimiteSaldo + 1m));
    }

    [Test]
    public void Constructor_ConTarjetaTipo_InicializaSaldoEnCeroYTipoCorrecto()
    {
        var tipo = new TarjetaTipo("Medio Boleto Estudiantil", 0.5m) { Id = 3 };
        var tarjeta = new Tarjeta(tipo);

        Assert.That(tarjeta.Saldo, Is.EqualTo(0m));
        Assert.That(tarjeta.TarjetaTipo, Is.SameAs(tipo));
        Assert.That(tarjeta.TarjetaTipoId, Is.EqualTo(3));
        Assert.That(tarjeta.EsNormal, Is.False);
        Assert.That(tarjeta.EsFranquicia, Is.True);
    }

    [Test]
    public void AgregarMulta_ConMontoPositivo_AcumulaEnMultasPendientes()
    {
        var tarjeta = new Tarjeta(5000m);
        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(0m));

        tarjeta.AgregarMulta(1250m);
        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(1250m));

        tarjeta.AgregarMulta(1250m);
        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(2500m));
    }

    [Test]
    public void AgregarMulta_ConMontoCeroONegativo_NoModificaMultasPendientes()
    {
        var tarjeta = new Tarjeta(5000m);
        tarjeta.AgregarMulta(1250m);

        tarjeta.AgregarMulta(0m);
        tarjeta.AgregarMulta(-500m);

        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(1250m));
    }

    [Test]
    public void LimpiarMultas_RestableceMultasPendientesACero()
    {
        var tarjeta = new Tarjeta(5000m);
        tarjeta.AgregarMulta(2500m);

        tarjeta.LimpiarMultas();

        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(0m));
    }

    [Test]
    public void AcreditarCarga_ConMontoInvalido_RetornaFalse()
    {
        var tarjeta = new Tarjeta(1000m);

        Assert.That(tarjeta.AcreditarCarga(0m), Is.False);
        Assert.That(tarjeta.AcreditarCarga(-500m), Is.False);
    }

    [Test]
    public void AcreditarCarga_SinSaldoPendiente_RetornaCero()
    {
        var tarjeta = new Tarjeta(1000m);

        decimal acreditado = tarjeta.AcreditarCarga();

        Assert.That(acreditado, Is.EqualTo(0m));
    }

    [Test]
    public void AcreditarCarga_ConSaldoAlMaximo_RetornaCeroYConservaSaldoPendiente()
    {
        var tarjeta = new Tarjeta(50000m);
        tarjeta.CargarSaldo(10000m);

        decimal acreditado = tarjeta.AcreditarCarga();

        Assert.That(acreditado, Is.EqualTo(0m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(Tarjeta.LimiteSaldo));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(4000m));
    }

    [Test]
    public void AcreditarCarga_ConMontoParametro_AcreditaHastaMaximoYGuardaExcedente()
    {
        var tarjeta = new Tarjeta(40000m);

        bool resultado = tarjeta.AcreditarCarga(20000m);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(4000m));
    }

    [Test]
    public void DescontarSaldo_LuegoDeViaje_AcreditaAutomaticamenteSaldoPendienteHastaMaximo()
    {
        var tarjeta = new Tarjeta(56000m);
        tarjeta.CargarSaldo(5000m);

        bool descontado = tarjeta.DescontarSaldo(2150m);

        Assert.That(descontado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(5000m - 2150m));
    }

    [Test]
    public void DescontarSaldo_ConSaldoPendienteMenorAlDescuento_AcreditaTodoElPendiente()
    {
        var tarjeta = new Tarjeta(56000m);
        tarjeta.CargarSaldo(2000m);

        bool descontado = tarjeta.DescontarSaldo(2150m);

        Assert.That(descontado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(55850m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(0m));
    }

    [Test]
    public void Colectivo_PagarCon_LuegoDeRealizarViaje_RecargaTarjetaConSaldoPendiente()
    {
        var colectivo = new Colectivo("144 Negra");
        var tarjeta = new Tarjeta(56000m) { Id = 100 };
        tarjeta.CargarSaldo(4000m);

        var boleto = colectivo.PagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(4000m - Colectivo.TarifaBasica));
        Assert.That(boleto!.SaldoRestante, Is.EqualTo(56000m));
    }

    [Test]
    public void Colectivo_PagarCon_ViajesConsecutivos_ConsumenSaldoPendienteHastaAgotarlo()
    {
        var colectivo = new Colectivo("102 Roja");
        var tarjeta = new Tarjeta(56000m);
        tarjeta.CargarSaldo(3000m);

        var boleto1 = colectivo.PagarCon(tarjeta);
        Assert.That(boleto1, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(850m));

        var boleto2 = colectivo.PagarCon(tarjeta);
        Assert.That(boleto2, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(54700m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(0m));
    }

    [Test]
    public void MedioBoleto_IntervaloMinimoCincoMinutos_ValidaLimitesExactos()
    {
        var tipo = TarjetaTipo.MedioBoleto;
        var tarjeta = new Tarjeta(5000m, tipo);
        var colectivo = new Colectivo("102");

        var primerBoleto = colectivo.PagarCon(tarjeta, FechaBaseLunes);
        Assert.That(primerBoleto, Is.Not.Null);

        bool puedeAntes = tarjeta.PuedeViajar(FechaBaseLunes.AddMinutes(4).AddSeconds(59), "102");
        Assert.That(puedeAntes, Is.False);

        bool puedeExacto = tarjeta.PuedeViajar(FechaBaseLunes.AddMinutes(5), "102");
        Assert.That(puedeExacto, Is.True);
    }

    [Test]
    public void TarjetaNormal_NoTieneRestriccionDeCincoMinutos()
    {
        var tarjeta = new Tarjeta(5000m, TarjetaTipo.Normal);
        var colectivo = new Colectivo("102 Roja");

        var fecha1 = new DateTime(2026, 10, 20, 10, 0, 0);
        var fecha2 = fecha1.AddMinutes(1);

        var boleto1 = colectivo.PagarCon(tarjeta, fecha1);
        var boleto2 = colectivo.PagarCon(tarjeta, fecha2);

        Assert.That(boleto1, Is.Not.Null);
        Assert.That(boleto2, Is.Not.Null);
    }

    [Test]
    public void MedioBoleto_PrimerosDosViajesMismoDia_PaganLaMitadDeTarifaNormal()
    {
        var tarjeta = new Tarjeta(5000m, TarjetaTipo.MedioBoleto);
        var colectivo = new Colectivo("102 Roja");

        var fecha1 = new DateTime(2026, 10, 20, 8, 0, 0);
        var fecha2 = new DateTime(2026, 10, 20, 12, 0, 0);

        var boleto1 = colectivo.PagarCon(tarjeta, fecha1);
        var boleto2 = colectivo.PagarCon(tarjeta, fecha2);

        decimal tarifaMedio = Colectivo.TarifaBasica / 2m;
        Assert.That(boleto1!.TotalAbonado, Is.EqualTo(tarifaMedio));
        Assert.That(boleto2!.TotalAbonado, Is.EqualTo(tarifaMedio));
    }

    [Test]
    public void MedioBoleto_TercerViajeMismoDia_SeCobraATarifaCompleta()
    {
        var tarjeta = new Tarjeta(10000m, TarjetaTipo.MedioBoleto);
        var colectivo = new Colectivo("102 Roja");

        var fecha1 = new DateTime(2026, 10, 20, 8, 0, 0);
        var fecha2 = new DateTime(2026, 10, 20, 10, 0, 0);
        var fecha3 = new DateTime(2026, 10, 20, 14, 0, 0);

        var boleto1 = colectivo.PagarCon(tarjeta, fecha1);
        var boleto2 = colectivo.PagarCon(tarjeta, fecha2);
        var boleto3 = colectivo.PagarCon(tarjeta, fecha3);

        decimal tarifaMedio = Colectivo.TarifaBasica / 2m;
        Assert.That(boleto1!.TotalAbonado, Is.EqualTo(tarifaMedio));
        Assert.That(boleto2!.TotalAbonado, Is.EqualTo(tarifaMedio));
        Assert.That(boleto3!.TotalAbonado, Is.EqualTo(Colectivo.TarifaBasica));
    }

    [Test]
    public void MedioBoleto_ViajesConDescuento_SeReinicianAlDiaSiguiente()
    {
        var tarjeta = new Tarjeta(10000m, TarjetaTipo.MedioBoleto);
        var colectivo = new Colectivo("102 Roja");

        var hoy = new DateTime(2026, 10, 20, 8, 0, 0);
        var mañana = new DateTime(2026, 10, 21, 8, 0, 0);

        colectivo.PagarCon(tarjeta, hoy);
        colectivo.PagarCon(tarjeta, hoy.AddMinutes(10));
        var tercerHoy = colectivo.PagarCon(tarjeta, hoy.AddMinutes(20));
        Assert.That(tercerHoy!.TotalAbonado, Is.EqualTo(Colectivo.TarifaBasica));

        var primerMañana = colectivo.PagarCon(tarjeta, mañana);
        Assert.That(primerMañana!.TotalAbonado, Is.EqualTo(Colectivo.TarifaBasica / 2m));
    }

    [Test]
    public void FranquiciaCompleta_PrimerosDosViajesMismoDia_SonGratuitos()
    {
        var tarjeta = new Tarjeta(5000m, TarjetaTipo.FranquiciaCompleta);
        var colectivo = new Colectivo("144 Negra");

        var fecha1 = new DateTime(2026, 10, 20, 8, 0, 0);
        var fecha2 = new DateTime(2026, 10, 20, 12, 0, 0);

        var boleto1 = colectivo.PagarCon(tarjeta, fecha1);
        var boleto2 = colectivo.PagarCon(tarjeta, fecha2);

        Assert.That(boleto1!.TotalAbonado, Is.EqualTo(0m));
        Assert.That(boleto2!.TotalAbonado, Is.EqualTo(0m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m));
    }

    [Test]
    public void FranquiciaCompleta_TercerViajeMismoDia_SeCobraATarifaCompleta()
    {
        var tarjeta = new Tarjeta(5000m, TarjetaTipo.FranquiciaCompleta);
        var colectivo = new Colectivo("K");

        var fecha1 = new DateTime(2026, 10, 20, 8, 0, 0);
        var fecha2 = new DateTime(2026, 10, 20, 10, 0, 0);
        var fecha3 = new DateTime(2026, 10, 20, 14, 0, 0);

        var boleto1 = colectivo.PagarCon(tarjeta, fecha1);
        var boleto2 = colectivo.PagarCon(tarjeta, fecha2);
        var boleto3 = colectivo.PagarCon(tarjeta, fecha3);

        Assert.That(boleto1!.TotalAbonado, Is.EqualTo(0m));
        Assert.That(boleto2!.TotalAbonado, Is.EqualTo(0m));
        Assert.That(boleto3!.TotalAbonado, Is.EqualTo(Colectivo.TarifaBasica));
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m - Colectivo.TarifaBasica));
    }

    [Test]
    public void FranquiciaCompleta_ViajesGratuitos_SeReinicianAlDiaSiguiente()
    {
        var tarjeta = new Tarjeta(5000m, TarjetaTipo.FranquiciaCompleta);
        var colectivo = new Colectivo("K");

        var hoy = new DateTime(2026, 10, 20, 8, 0, 0);
        var mañana = new DateTime(2026, 10, 21, 8, 0, 0);

        colectivo.PagarCon(tarjeta, hoy);
        colectivo.PagarCon(tarjeta, hoy.AddMinutes(10));
        colectivo.PagarCon(tarjeta, hoy.AddMinutes(20));

        var primerMañana = colectivo.PagarCon(tarjeta, mañana);
        Assert.That(primerMañana!.TotalAbonado, Is.EqualTo(0m));
    }

    [Test]
    public void BoletoGratuitoEstudiantil_TieneMismoComportamientoQueFranquiciaCompleta()
    {
        var tarjeta = new Tarjeta(5000m, TarjetaTipo.BoletoGratuito);
        var colectivo = new Colectivo("144 Negra");

        var fecha1 = new DateTime(2026, 10, 20, 8, 0, 0);
        var fecha2 = new DateTime(2026, 10, 20, 10, 0, 0);
        var fecha3 = new DateTime(2026, 10, 20, 14, 0, 0);

        var boleto1 = colectivo.PagarCon(tarjeta, fecha1);
        var boleto2 = colectivo.PagarCon(tarjeta, fecha2);
        var boleto3 = colectivo.PagarCon(tarjeta, fecha3);

        Assert.That(boleto1!.TotalAbonado, Is.EqualTo(0m));
        Assert.That(boleto2!.TotalAbonado, Is.EqualTo(0m));
        Assert.That(boleto3!.TotalAbonado, Is.EqualTo(Colectivo.TarifaBasica));
    }

    [Test]
    public void UsoFrecuente_Viajes1Al29_PaganTarifaNormalSinDescuento()
    {
        var tarjeta = new Tarjeta(40000m);
        var fechaBase = new DateTime(2026, 10, 1, 8, 0, 0);

        for (int i = 0; i < 28; i++)
        {
            tarjeta.RegistrarViaje(fechaBase.AddHours(i));
        }

        decimal tarifaViaje29 = tarjeta.CalcularPrecio(Colectivo.TarifaBasica, fechaBase);
        Assert.That(tarifaViaje29, Is.EqualTo(2150m));
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

        decimal tarifaViaje30 = tarjeta.CalcularPrecio(Colectivo.TarifaBasica, fechaBase);
        Assert.That(tarifaViaje30, Is.EqualTo(2150m * 0.80m));

        for (int i = 29; i < 78; i++)
        {
            tarjeta.RegistrarViaje(fechaBase.AddHours(i));
        }

        decimal tarifaViaje79 = tarjeta.CalcularPrecio(Colectivo.TarifaBasica, fechaBase);
        Assert.That(tarifaViaje79, Is.EqualTo(2150m * 0.80m));
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

        decimal tarifaViaje80 = tarjeta.CalcularPrecio(Colectivo.TarifaBasica, fechaBase);
        Assert.That(tarifaViaje80, Is.EqualTo(2150m * 0.75m));
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
        Assert.That(tarifaNoviembre, Is.EqualTo(2150m));
    }

    [Test]
    public void UsoFrecuente_NoAplicaAFranquicias_AplicaSoloATarjetaNormal()
    {
        var tarjetaMedio = new Tarjeta(40000m, TarjetaTipo.MedioBoleto);
        var fecha = new DateTime(2026, 10, 20, 10, 0, 0);

        for (int i = 1; i <= 25; i++)
        {
            tarjetaMedio.RegistrarViaje(new DateTime(2026, 10, 1, 8, 0, 0).AddDays(i % 10).AddMinutes(i));
        }

        decimal tarifa = tarjetaMedio.CalcularPrecio(Colectivo.TarifaBasica, fecha);
        Assert.That(tarifa, Is.EqualTo(2150m / 2m));
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
    public void SeguimientoTarifaBici_RegistraYVerificaFechas()
    {
        var tarjeta = new Tarjeta(10000m);
        var fecha1 = new DateTime(2026, 10, 20, 9, 0, 0);
        var fecha2 = new DateTime(2026, 10, 21, 9, 0, 0);

        Assert.That(tarjeta.TieneTarifaBiciPagada(fecha1), Is.False);

        tarjeta.RegistrarPagoTarifaBici(fecha1);

        Assert.That(tarjeta.TieneTarifaBiciPagada(fecha1), Is.True);
        Assert.That(tarjeta.TieneTarifaBiciPagada(fecha2), Is.False);
    }

    [Test]
    public void HistorialViajes_RegistraViajesYUltimoViaje()
    {
        var tarjeta = new Tarjeta(10000m);
        Assert.That(tarjeta.UltimoViaje, Is.Null);
        Assert.That(tarjeta.HistorialViajes, Is.Empty);

        tarjeta.RegistrarViaje(FechaBaseLunes);

        Assert.That(tarjeta.UltimoViaje, Is.EqualTo(FechaBaseLunes));
        Assert.That(tarjeta.HistorialViajes, Has.Count.EqualTo(1));
        Assert.That(tarjeta.HistorialViajes[0], Is.EqualTo(FechaBaseLunes));
    }
}
