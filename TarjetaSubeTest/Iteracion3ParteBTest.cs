namespace TarjetaSubeTest;

using NUnit.Framework;
using TarjetaSube;

[TestFixture]
public class Iteracion3ParteBTest
{
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

    [Test]
    public void CargarSaldo_QueSuperaLimiteMaximo_AcreditaHasta56000YGuardaExcedenteEnSaldoPendiente()
    {
        var tarjeta = new Tarjeta(50000m);

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

        tarjeta.CargarSaldo(10000m);
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(4000m));

        tarjeta.CargarSaldo(8000m);
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(12000m));
    }

    [Test]
    public void AcreditarCarga_SinParametros_AcreditaSaldoPendienteHastaElMaximo()
    {
        var tarjeta = new Tarjeta(50000m);
        tarjeta.CargarSaldo(10000m);

        tarjeta.DescontarSaldo(3000m);
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(1000m));

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
}
