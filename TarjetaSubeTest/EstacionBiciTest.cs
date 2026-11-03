namespace TarjetaSubeTest;

using NUnit.Framework;
using TarjetaSube;

[TestFixture]
public class EstacionBiciTest
{
    private EstacionBici _estacion = null!;
    private TarjetaTipo _tipoMedioBoleto = null!;
    private TarjetaTipo _tipoGratuito = null!;
    private TarjetaTipo _tipoFranquiciaCompleta = null!;

    [SetUp]
    public void Setup()
    {
        _estacion = new EstacionBici("Estación Pellegrini");
        _tipoMedioBoleto = new TarjetaTipo(TarjetaTipo.NombreMedioBoleto, 0.5m);
        _tipoGratuito = new TarjetaTipo(TarjetaTipo.NombreBoletoGratuito, 0m);
        _tipoFranquiciaCompleta = new TarjetaTipo(TarjetaTipo.NombreFranquiciaCompleta, 0m);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // 1. RETIRO EXITOSO Y COBRO DE TARIFA FIJA DE $2418,75
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public void TarifaServicio_EsDosMilCuatrocientosDieciochoConSetentaYCinco()
    {
        Assert.That(EstacionBici.TarifaServicio, Is.EqualTo(2418.75m));
        Assert.That(EstacionBici.TarifaDiaria, Is.EqualTo(2418.75m));
        Assert.That(EstacionBici.TarifaRetiro, Is.EqualTo(2418.75m));
    }

    [Test]
    public void RetirarBici_ConSaldoSuficiente_DebitaTarifaDiariaCorrecta()
    {
        var tarjeta = new Tarjeta(5000m);
        var fecha = new DateTime(2026, 10, 28, 8, 30, 0);

        bool exito = _estacion.RetirarBici(tarjeta, fecha);

        Assert.That(exito, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m - 2418.75m));
    }

    [Test]
    public void RetirarBici_ConSaldoExacto_PermiteRetiroYSaldoQuedaEnCero()
    {
        var tarjeta = new Tarjeta(2418.75m);
        var fecha = new DateTime(2026, 10, 28, 9, 0, 0);

        bool exito = _estacion.RetirarBici(tarjeta, fecha);

        Assert.That(exito, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(0m));
    }

    [Test]
    public void RetirarBici_UsandoSobrecargaSinFecha_DebitaTarifaCorrectamente()
    {
        var tarjeta = new Tarjeta(5000m);

        bool exito = _estacion.RetirarBici(tarjeta);

        Assert.That(exito, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m - 2418.75m));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // 2. CONTROL DE COBRO ÚNICO POR DÍA
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public void RetirarBici_MultiplesVecesEnElMismoDia_SoloCobraTarifaUnaVez()
    {
        var tarjeta = new Tarjeta(10000m);
        var primerRetiro = new DateTime(2026, 10, 28, 8, 0, 0);
        var segundoRetiro = new DateTime(2026, 10, 28, 12, 30, 0);
        var tercerRetiro = new DateTime(2026, 10, 28, 17, 45, 0);

        bool primerExito = _estacion.RetirarBici(tarjeta, primerRetiro);
        Assert.That(primerExito, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(10000m - 2418.75m));

        // Devolvemos la bici antes del segundo retiro
        _estacion.DevolverBicicleta(tarjeta, primerRetiro.AddMinutes(30));

        bool segundoExito = _estacion.RetirarBici(tarjeta, segundoRetiro);
        Assert.That(segundoExito, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(10000m - 2418.75m));

        _estacion.DevolverBicicleta(tarjeta, segundoRetiro.AddMinutes(30));

        bool tercerExito = _estacion.RetirarBici(tarjeta, tercerRetiro);
        Assert.That(tercerExito, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(10000m - 2418.75m));
    }

    [Test]
    public void RetirarBici_EnDiasDiferentes_CobraTarifaDiariaEnCadaDia()
    {
        var tarjeta = new Tarjeta(10000m);
        var dia1 = new DateTime(2026, 10, 28, 9, 0, 0);
        var dia2 = new DateTime(2026, 10, 29, 9, 0, 0);

        bool exitoDia1 = _estacion.RetirarBici(tarjeta, dia1);
        Assert.That(exitoDia1, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(10000m - 2418.75m));

        _estacion.DevolverBicicleta(tarjeta, dia1.AddMinutes(30));

        bool exitoDia2 = _estacion.RetirarBici(tarjeta, dia2);
        Assert.That(exitoDia2, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(10000m - (2418.75m * 2)));
    }

    [Test]
    public void YaPagoTarifaDiaria_RetornaTrueSiYaAbonoEnLaFechaYFalseSiNo()
    {
        var tarjeta = new Tarjeta(5000m);
        var hoy = new DateTime(2026, 10, 28, 10, 0, 0);
        var manana = new DateTime(2026, 10, 29, 10, 0, 0);

        Assert.That(_estacion.YaPagoTarifaDiaria(tarjeta, hoy), Is.False);

        _estacion.RetirarBici(tarjeta, hoy);

        Assert.That(_estacion.YaPagoTarifaDiaria(tarjeta, hoy), Is.True);
        Assert.That(_estacion.YaPagoTarifaDiaria(tarjeta, manana), Is.False);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // 3. SIN DESCUENTOS POR FRANQUICIA (TARIFA COMPLETA PARA TODAS)
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public void RetirarBici_TarjetaMedioBoleto_PagaTarifaCompletaSinDescuento()
    {
        var tarjeta = new Tarjeta(5000m, _tipoMedioBoleto);
        var fecha = new DateTime(2026, 10, 28, 10, 0, 0);

        bool exito = _estacion.RetirarBici(tarjeta, fecha);

        Assert.That(exito, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m - 2418.75m));
    }

    [Test]
    public void RetirarBici_TarjetaBoletoGratuito_PagaTarifaCompletaSinDescuento()
    {
        var tarjeta = new Tarjeta(5000m, _tipoGratuito);
        var fecha = new DateTime(2026, 10, 28, 10, 0, 0);

        bool exito = _estacion.RetirarBici(tarjeta, fecha);

        Assert.That(exito, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m - 2418.75m));
    }

    [Test]
    public void RetirarBici_TarjetaFranquiciaCompleta_PagaTarifaCompletaSinDescuento()
    {
        var tarjeta = new Tarjeta(5000m, _tipoFranquiciaCompleta);
        var fecha = new DateTime(2026, 10, 28, 10, 0, 0);

        bool exito = _estacion.RetirarBici(tarjeta, fecha);

        Assert.That(exito, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m - 2418.75m));
    }

    [Test]
    public void RetirarBici_TarjetaFranquicia_PuedeAbonarCualquierDiaYHorario()
    {
        var tarjeta = new Tarjeta(5000m, _tipoMedioBoleto);
        var domingoNoche = new DateTime(2026, 10, 25, 23, 30, 0);

        bool exito = _estacion.RetirarBici(tarjeta, domingoNoche);

        Assert.That(exito, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m - 2418.75m));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // 4. RECHAZO DE RETIRO POR FALTA DE SALDO (Y LÍMITE NEGATIVO)
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public void RetirarBici_SinSaldoSuficienteSuperandoLimiteNegativo_RechazaElRetiro()
    {
        var tarjeta = new Tarjeta(0m);
        var fecha = new DateTime(2026, 10, 28, 10, 0, 0);

        bool exito = _estacion.RetirarBici(tarjeta, fecha);

        Assert.That(exito, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(0m));
        Assert.That(_estacion.YaPagoTarifaDiaria(tarjeta, fecha), Is.False);
    }

    [Test]
    public void RetirarBici_SaldoInsuficienteParaCubrirHastaElLimiteNegativo_RechazaElRetiro()
    {
        var tarjeta = new Tarjeta(300m);
        var fecha = new DateTime(2026, 10, 28, 10, 0, 0);

        bool exito = _estacion.RetirarBici(tarjeta, fecha);

        Assert.That(exito, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(300m));
    }

    [Test]
    public void RetirarBici_ConSaldoQueQuedaExactamenteEnLimiteNegativo_PermiteRetiro()
    {
        var tarjeta = new Tarjeta(418.75m);
        var fecha = new DateTime(2026, 10, 28, 10, 0, 0);

        bool exito = _estacion.RetirarBici(tarjeta, fecha);

        Assert.That(exito, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-2000m));
    }

    [Test]
    public void RetirarBici_ConSaldoParcialUsandoSaldoNegativoValido_PermiteRetiro()
    {
        var tarjeta = new Tarjeta(1000m);
        var fecha = new DateTime(2026, 10, 28, 10, 0, 0);

        bool exito = _estacion.RetirarBici(tarjeta, fecha);

        Assert.That(exito, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-1418.75m));
    }

    [Test]
    public void RetirarBici_ConSaldoNegativoPrevioQueSuperaLimite_RechazaElRetiro()
    {
        var tarjeta = new Tarjeta(1000m);
        tarjeta.DescontarSaldo(1500m);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-500m));

        var fecha = new DateTime(2026, 10, 28, 10, 0, 0);

        bool exito = _estacion.RetirarBici(tarjeta, fecha);

        Assert.That(exito, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-500m));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // 5. SOPORTE DE MULTAS ACUMULADAS AL RETIRAR BICI
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public void RetirarBici_ConUnaMultaAcumulada_CobraTarifaMasMulta()
    {
        var tarjeta = new Tarjeta(5000m);
        tarjeta.AgregarMulta(1250m);
        var fecha = new DateTime(2026, 10, 28, 10, 0, 0);

        Assert.That(_estacion.ObtenerMultasPendientes(tarjeta), Is.EqualTo(1250m));

        bool exito = _estacion.RetirarBici(tarjeta, fecha);

        Assert.That(exito, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m - 3668.75m));
        Assert.That(_estacion.ObtenerMultasPendientes(tarjeta), Is.EqualTo(0m));
    }

    [Test]
    public void RetirarBici_ConVariasMultasAcumuladas_CobraTodasLasMultas()
    {
        var tarjeta = new Tarjeta(10000m);
        tarjeta.AgregarMulta(1250m);
        tarjeta.AgregarMulta(1250m);
        var fecha = new DateTime(2026, 10, 28, 10, 0, 0);

        Assert.That(_estacion.ObtenerMultasPendientes(tarjeta), Is.EqualTo(2500m));

        bool exito = _estacion.RetirarBici(tarjeta, fecha);

        Assert.That(exito, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(10000m - 4918.75m));
        Assert.That(_estacion.ObtenerMultasPendientes(tarjeta), Is.EqualTo(0m));
    }

    [Test]
    public void RetirarBici_HabiendoPagadoTarifaDiariaPeroConMultaPendiente_CobraSoloLaMulta()
    {
        var tarjeta = new Tarjeta(5000m);
        var fecha1 = new DateTime(2026, 10, 28, 8, 0, 0);
        var fecha2 = new DateTime(2026, 10, 28, 15, 0, 0);

        _estacion.RetirarBici(tarjeta, fecha1);
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m - 2418.75m));

        // Devolvemos la bici con exceso de tiempo → genera multa
        _estacion.DevolverBicicleta(tarjeta, fecha1.AddMinutes(75));
        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(1250m));

        bool exito = _estacion.RetirarBici(tarjeta, fecha2);

        Assert.That(exito, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m - 2418.75m - 1250m));
        Assert.That(_estacion.ObtenerMultasPendientes(tarjeta), Is.EqualTo(0m));
    }

    [Test]
    public void RetirarBici_ConSaldoInsuficienteTeniendoMultaPendiente_RechazaElRetiro()
    {
        var tarjeta = new Tarjeta(1000m);
        tarjeta.AgregarMulta(1250m);
        var fecha = new DateTime(2026, 10, 28, 10, 0, 0);

        bool exito = _estacion.RetirarBici(tarjeta, fecha);

        Assert.That(exito, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(1000m));
        Assert.That(_estacion.ObtenerMultasPendientes(tarjeta), Is.EqualTo(1250m));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // 6. INTEGRACIÓN CON SALDO PENDIENTE Y ACREDITACIÓN AUTOMÁTICA
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public void RetirarBici_ConSaldoPendiente_AcreditaSaldoAutomaticamenteAlCobrar()
    {
        var tarjeta = new Tarjeta(56000m);
        tarjeta.AcreditarCarga(5000m);
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(5000m));

        var fecha = new DateTime(2026, 10, 28, 10, 0, 0);

        bool exito = _estacion.RetirarBici(tarjeta, fecha);

        Assert.That(exito, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(5000m - 2418.75m));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // 7. DEVOLUCIÓN Y CONTROL DE TIEMPO (1 HORA MÁXIMO)
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public void DevolverBicicleta_DentroDeUnaHora_NoGeneraMulta()
    {
        var tarjeta = new Tarjeta(5000m);
        var fechaRetiro = new DateTime(2026, 10, 28, 10, 0, 0);

        _estacion.RetirarBicicleta(tarjeta, fechaRetiro);
        _estacion.DevolverBicicleta(tarjeta, fechaRetiro.AddMinutes(45));

        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(0m));
        Assert.That(_estacion.EstaEnUso, Is.False);
    }

    [Test]
    public void DevolverBicicleta_EnLimiteExactoDeUnaHora_NoGeneraMulta()
    {
        var tarjeta = new Tarjeta(5000m);
        var fechaRetiro = new DateTime(2026, 10, 28, 10, 0, 0);

        _estacion.RetirarBicicleta(tarjeta, fechaRetiro);
        _estacion.DevolverBicicleta(tarjeta, fechaRetiro.AddHours(1));

        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(0m));
    }

    [Test]
    public void DevolverBicicleta_ExcediendoUnaHora_AplicaMultaDe1250()
    {
        var tarjeta = new Tarjeta(10000m);
        var fechaRetiro = new DateTime(2026, 10, 28, 10, 0, 0);

        _estacion.RetirarBicicleta(tarjeta, fechaRetiro);
        _estacion.DevolverBicicleta(tarjeta, fechaRetiro.AddMinutes(75));

        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(1250m));
    }

    [Test]
    public void DevolverBicicleta_SinRetiroActivo_RetornaFalse()
    {
        var tarjeta = new Tarjeta(5000m);
        var fecha = new DateTime(2026, 10, 28, 10, 0, 0);

        bool resultado = _estacion.DevolverBicicleta(tarjeta, fecha);

        Assert.That(resultado, Is.False);
    }

    [Test]
    public void EstaEnUso_TrasRetirar_RetornaTrue()
    {
        var tarjeta = new Tarjeta(5000m);
        var fecha = new DateTime(2026, 10, 28, 10, 0, 0);

        Assert.That(_estacion.EstaEnUso, Is.False);
        _estacion.RetirarBicicleta(tarjeta, fecha);
        Assert.That(_estacion.EstaEnUso, Is.True);
        Assert.That(_estacion.FechaRetiro, Is.EqualTo(fecha));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // 8. COMPATIBILIDAD CON SUBCLASE MiBiciTuBici Y VALIDACIONES
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public void MiBiciTuBici_InstanciaHeredada_FuncionaExactamenteIgual()
    {
        var miBici = new MiBiciTuBici("Estación Central");
        var tarjeta = new Tarjeta(5000m);
        var fecha = new DateTime(2026, 10, 28, 10, 0, 0);

        bool exito = miBici.RetirarBici(tarjeta, fecha);

        Assert.That(exito, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m - 2418.75m));
        Assert.That(miBici.Nombre, Is.EqualTo("Estación Central"));
    }

    [Test]
    public void RetirarBici_ConTarjetaNula_RetornaFalse()
    {
        bool exito = _estacion.RetirarBici(null!);
        Assert.That(exito, Is.False);
    }
}
