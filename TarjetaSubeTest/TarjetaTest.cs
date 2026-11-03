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
    public void CargarSaldo_SuperandoLimiteMaximo_AcreditaHastaMaximoYDejaExcedenteEnSaldoPendiente()
    {
        var tarjeta = new Tarjeta(50000m);

        bool resultado = tarjeta.CargarSaldo(10000m);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(Tarjeta.LimiteSaldo));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(4000m));
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
        tarjeta.CargarSaldo(10000m); // Saldo: 56000, SaldoPendiente: 4000

        decimal acreditado = tarjeta.AcreditarCarga();

        Assert.That(acreditado, Is.EqualTo(0m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(Tarjeta.LimiteSaldo));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(4000m));
    }

    [Test]
    public void MedioBoleto_IntervaloMinimoCincoMinutos_ValidaLimitesExactos()
    {
        var tipo = TarjetaTipo.MedioBoleto;
        var tarjeta = new Tarjeta(5000m, tipo);
        var colectivo = new Colectivo("102");

        var primerBoleto = colectivo.PagarCon(tarjeta, FechaBaseLunes);
        Assert.That(primerBoleto, Is.Not.Null);

        // Intento a los 4 minutos y 59 segundos: NO puede viajar
        bool puedeAntes = tarjeta.PuedeViajar(FechaBaseLunes.AddMinutes(4).AddSeconds(59), "102");
        Assert.That(puedeAntes, Is.False);

        // Intento exactamente a los 5 minutos: SÍ puede viajar
        bool puedeExacto = tarjeta.PuedeViajar(FechaBaseLunes.AddMinutes(5), "102");
        Assert.That(puedeExacto, Is.True);
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
