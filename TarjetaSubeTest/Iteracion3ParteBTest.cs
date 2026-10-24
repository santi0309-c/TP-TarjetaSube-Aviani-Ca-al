namespace TarjetaSubeTest;

using NUnit.Framework;
using TarjetaSube;

[TestFixture]
public class Iteracion3ParteBTest
{
    // ═════════════════════════════════════════════════════════════════════════
    // 1. LÍMITE MÁXIMO DE SALDO ($56.000)
    // ═════════════════════════════════════════════════════════════════════════

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
    public void Constructor_SuperandoNuevoLimiteMaximo56000_LanzaExcepcion()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Tarjeta(56001m));
        Assert.That(ex!.Message, Does.Contain("56000"));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // 2. CARGAR SALDO Y SALDO PENDIENTE
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public void CargarSaldo_QueSuperaLimiteMaximo_AcreditaHasta56000YGuardaExcedenteEnSaldoPendiente()
    {
        var tarjeta = new Tarjeta(50000m);

        // 50000 + 10000 = 60000 > 56000
        bool resultado = tarjeta.CargarSaldo(10000m);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
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

        // Primera carga: 10.000 -> 6.000 van a saldo (llega a 56.000) y 4.000 a pendiente
        tarjeta.CargarSaldo(10000m);
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(4000m));

        // Segunda carga: 8.000 -> los 8.000 van a pendiente
        tarjeta.CargarSaldo(8000m);
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(12000m));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // 3. MÉTODO AcreditarCarga
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public void AcreditarCarga_SinParametros_AcreditaSaldoPendienteHastaElMaximo()
    {
        var tarjeta = new Tarjeta(50000m);
        tarjeta.CargarSaldo(10000m); // Saldo: 56.000, Pendiente: 4.000

        // Si se descuentan 3000 de saldo manualmente (queda en 53.000)
        tarjeta.DescontarSaldo(3000m);
        // DescontarSaldo ya acredita automáticamente 3000, dejando Saldo en 56000 y Pendiente en 1000.
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(1000m));

        // Llamar a AcreditarCarga() cuando ya está al máximo no modifica nada
        decimal acreditado = tarjeta.AcreditarCarga();
        Assert.That(acreditado, Is.EqualTo(0m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(1000m));
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

    [TestCase(0)]
    [TestCase(-500)]
    public void AcreditarCarga_ConMontoInvalido_RetornaFalse(decimal monto)
    {
        var tarjeta = new Tarjeta(10000m);

        bool resultado = tarjeta.AcreditarCarga(monto);

        Assert.That(resultado, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(10000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(0m));
    }

    [Test]
    public void AcreditarCarga_SinSaldoPendiente_RetornaCeroYNoModificaSaldo()
    {
        var tarjeta = new Tarjeta(20000m);

        decimal acreditado = tarjeta.AcreditarCarga();

        Assert.That(acreditado, Is.EqualTo(0m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(20000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(0m));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // 4. ACREDITACIÓN TRAS VIAJES (DescontarSaldo Y Colectivo.PagarCon)
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public void DescontarSaldo_LuegoDeViaje_AcreditaAutomaticamenteSaldoPendienteHastaMaximo()
    {
        var tarjeta = new Tarjeta(56000m);
        tarjeta.CargarSaldo(5000m); // Saldo: 56.000, Pendiente: 5.000

        // Viaje de tarifa básica ($2150)
        bool descontado = tarjeta.DescontarSaldo(2150m);

        Assert.That(descontado, Is.True);
        // Saldo bajó a 53850 y se acreditaron 2150 del pendiente, volviendo al máximo de 56000
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        // Pendiente bajó de 5000 a 2850
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(5000m - 2150m));
    }

    [Test]
    public void DescontarSaldo_ConSaldoPendienteMenorAlDescuento_AcreditaTodoElPendiente()
    {
        var tarjeta = new Tarjeta(56000m);
        tarjeta.CargarSaldo(2000m); // Saldo: 56.000, Pendiente: 2.000

        // Se descuenta 2150 (mayor que el saldo pendiente de 2000)
        bool descontado = tarjeta.DescontarSaldo(2150m);

        Assert.That(descontado, Is.True);
        // 56000 - 2150 + 2000 = 55850
        Assert.That(tarjeta.Saldo, Is.EqualTo(55850m));
        // Se acreditó todo el pendiente
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(0m));
    }

    [Test]
    public void Colectivo_PagarCon_LuegoDeRealizarViaje_RecargaTarjetaConSaldoPendiente()
    {
        var colectivo = new Colectivo("144 Negra");
        var tarjeta = new Tarjeta(56000m) { Id = 100 };
        tarjeta.CargarSaldo(4000m); // Saldo: 56.000, Pendiente: 4.000

        var boleto = colectivo.PagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        // La tarjeta volvió a recargarse hasta 56.000
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(4000m - Colectivo.TarifaBasica));
        Assert.That(boleto!.SaldoRestante, Is.EqualTo(56000m));
    }

    [Test]
    public void Colectivo_PagarCon_ViajesConsecutivos_ConsumenSaldoPendienteHastaAgotarlo()
    {
        var colectivo = new Colectivo("102 Roja");
        var tarjeta = new Tarjeta(56000m);
        tarjeta.CargarSaldo(3000m); // Saldo: 56.000, Pendiente: 3.000

        // 1er viaje: tarifa 2150. Pendiente tenía 3000 -> se acreditan 2150.
        // Queda Saldo = 56000, Pendiente = 850.
        var boleto1 = colectivo.PagarCon(tarjeta);
        Assert.That(boleto1, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(850m));

        // 2do viaje: tarifa 2150. Pendiente tenía 850 -> se acreditan 850 (se agota).
        // Saldo queda en 56000 - 2150 + 850 = 54700, Pendiente = 0.
        var boleto2 = colectivo.PagarCon(tarjeta);
        Assert.That(boleto2, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(54700m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(0m));
    }
}
