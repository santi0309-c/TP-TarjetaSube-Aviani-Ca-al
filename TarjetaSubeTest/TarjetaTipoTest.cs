namespace TarjetaSubeTest;

using NUnit.Framework;
using TarjetaSube;

[TestFixture]
public class TarjetaTipoTest
{
    private static readonly DateTime FechaHabil = new(2026, 10, 20, 10, 0, 0);

    [Test]
    public void FranquiciaCompleta_SiemprePuedePagar_InclusoConSaldoCero()
    {
        var tipoFranquiciaCompleta = new TarjetaTipo("Franquicia Completa", 0m);
        var tarjeta = new Tarjeta(0m, tipoFranquiciaCompleta) { Id = 10 };
        var colectivo = new Colectivo("144 Negra");

        var boleto = colectivo.PagarCon(tarjeta, FechaHabil);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.MontoPagado, Is.EqualTo(0m));
        Assert.That(boleto.SaldoRestante, Is.EqualTo(0m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(0m));
    }

    [Test]
    public void MedioBoleto_PagaExactamenteLaMitadDeTarifaNormal()
    {
        var tipoMedioBoleto = new TarjetaTipo("Medio Boleto Estudiantil", 0.5m);
        var tarjeta = new Tarjeta(2000m, tipoMedioBoleto) { Id = 20 };
        var colectivo = new Colectivo("102 Roja");

        decimal tarifaEsperada = Colectivo.TarifaBasica / 2m;
        var boleto = colectivo.PagarCon(tarjeta, FechaHabil);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.MontoPagado, Is.EqualTo(tarifaEsperada));
        Assert.That(boleto.SaldoRestante, Is.EqualTo(2000m - tarifaEsperada));
        Assert.That(tarjeta.Saldo, Is.EqualTo(2000m - tarifaEsperada));
    }

    [Test]
    public void BoletoGratuitoEstudiantil_PagaCeroYEmiteBoletoCorrecto()
    {
        var tipoGratuito = new TarjetaTipo("Boleto Gratuito Estudiantil", 0m);
        var tarjeta = new Tarjeta(100m, tipoGratuito);
        var colectivo = new Colectivo("K");

        var boleto = colectivo.PagarCon(tarjeta, FechaHabil);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.MontoPagado, Is.EqualTo(0m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(100m));
    }

    [Test]
    public void TarjetaNormal_PagaTarifaCompleta()
    {
        var tipoNormal = new TarjetaTipo("Normal", 1m);
        var tarjeta = new Tarjeta(3000m, tipoNormal);
        var colectivo = new Colectivo("115");

        var boleto = colectivo.PagarCon(tarjeta, FechaHabil);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.MontoPagado, Is.EqualTo(Colectivo.TarifaBasica));
        Assert.That(tarjeta.Saldo, Is.EqualTo(3000m - Colectivo.TarifaBasica));
    }

    [Test]
    public void AgregarNuevoTipo_SinCrearNuevaClase_CalculaPrecioYPermitePagar()
    {
        var tipoTarifaSocial = new TarjetaTipo("Tarifa Social", 0.45m);
        var tarjeta = new Tarjeta(2000m, tipoTarifaSocial);
        var colectivo = new Colectivo("122 Verde");

        decimal tarifaEsperada = Colectivo.TarifaBasica * 0.45m;
        var boleto = colectivo.PagarCon(tarjeta, FechaHabil);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.MontoPagado, Is.EqualTo(tarifaEsperada));
        Assert.That(tarjeta.Saldo, Is.EqualTo(2000m - tarifaEsperada));
    }

    [Test]
    public void CalcularPrecio_SinTipoAsignado_DevuelveTarifaBase()
    {
        var tarjeta = new Tarjeta(2000m);

        decimal precio = tarjeta.CalcularPrecio(Colectivo.TarifaBasica);

        Assert.That(precio, Is.EqualTo(Colectivo.TarifaBasica));
    }

    [Test]
    public void FactoriesEstaticos_CreanInstanciasConValoresEsperados()
    {
        var normal = TarjetaTipo.Normal;
        Assert.That(normal.Nombre, Is.EqualTo(TarjetaTipo.NombreNormal));
        Assert.That(normal.Multiplicador, Is.EqualTo(1.0m));
        Assert.That(normal.EsNormal, Is.True);
        Assert.That(normal.EsFranquicia, Is.False);

        var medio = TarjetaTipo.MedioBoleto;
        Assert.That(medio.Nombre, Is.EqualTo(TarjetaTipo.NombreMedioBoleto));
        Assert.That(medio.Multiplicador, Is.EqualTo(0.5m));
        Assert.That(medio.EsMedioBoleto, Is.True);

        var completa = TarjetaTipo.FranquiciaCompleta;
        Assert.That(completa.Nombre, Is.EqualTo(TarjetaTipo.NombreFranquiciaCompleta));
        Assert.That(completa.Multiplicador, Is.EqualTo(0.0m));
        Assert.That(completa.EsFranquiciaCompleta, Is.True);

        var gratuito = TarjetaTipo.BoletoGratuito;
        Assert.That(gratuito.Nombre, Is.EqualTo(TarjetaTipo.NombreBoletoGratuito));
        Assert.That(gratuito.Multiplicador, Is.EqualTo(0.0m));
        Assert.That(gratuito.EsFranquiciaCompleta, Is.True);
    }

    [Test]
    public void CalcularTarifa_TipoNormal_AplicaDescuentosPorUsoFrecuente()
    {
        var tipo = TarjetaTipo.Normal;
        decimal tarifaBase = 2150m;

        // Viajes 1 al 29: 100% de la tarifa
        Assert.That(tipo.CalcularTarifa(tarifaBase, viajesHoy: 0, viajesMes: 0), Is.EqualTo(2150m));
        Assert.That(tipo.CalcularTarifa(tarifaBase, viajesHoy: 5, viajesMes: 28), Is.EqualTo(2150m));

        // Viajes 30 al 79: 20% de descuento (paga el 80%)
        Assert.That(tipo.CalcularTarifa(tarifaBase, viajesHoy: 1, viajesMes: 29), Is.EqualTo(2150m * 0.80m));
        Assert.That(tipo.CalcularTarifa(tarifaBase, viajesHoy: 3, viajesMes: 50), Is.EqualTo(2150m * 0.80m));
        Assert.That(tipo.CalcularTarifa(tarifaBase, viajesHoy: 2, viajesMes: 78), Is.EqualTo(2150m * 0.80m));

        // Viajes 80 en adelante: 25% de descuento (paga el 75%)
        Assert.That(tipo.CalcularTarifa(tarifaBase, viajesHoy: 1, viajesMes: 79), Is.EqualTo(2150m * 0.75m));
        Assert.That(tipo.CalcularTarifa(tarifaBase, viajesHoy: 4, viajesMes: 120), Is.EqualTo(2150m * 0.75m));
    }

    [Test]
    public void CalcularTarifa_MedioBoleto_AplicaLimiteDeDosViajesBonificadosPorDia()
    {
        var tipo = TarjetaTipo.MedioBoleto;
        decimal tarifaBase = 2150m;

        // Primeros 2 viajes: paga el 50%
        Assert.That(tipo.CalcularTarifa(tarifaBase, viajesHoy: 0), Is.EqualTo(1075m));
        Assert.That(tipo.CalcularTarifa(tarifaBase, viajesHoy: 1), Is.EqualTo(1075m));

        // A partir del tercer viaje en el mismo día: tarifa completa
        Assert.That(tipo.CalcularTarifa(tarifaBase, viajesHoy: 2), Is.EqualTo(2150m));
        Assert.That(tipo.CalcularTarifa(tarifaBase, viajesHoy: 3), Is.EqualTo(2150m));
    }

    [Test]
    public void CalcularTarifa_FranquiciaCompleta_AplicaLimiteDeDosViajesBonificadosPorDia()
    {
        var tipo = TarjetaTipo.FranquiciaCompleta;
        decimal tarifaBase = 2150m;

        // Primeros 2 viajes: 100% bonificado (paga 0)
        Assert.That(tipo.CalcularTarifa(tarifaBase, viajesHoy: 0), Is.EqualTo(0m));
        Assert.That(tipo.CalcularTarifa(tarifaBase, viajesHoy: 1), Is.EqualTo(0m));

        // A partir del tercer viaje: tarifa completa
        Assert.That(tipo.CalcularTarifa(tarifaBase, viajesHoy: 2), Is.EqualTo(2150m));
        Assert.That(tipo.CalcularTarifa(tarifaBase, viajesHoy: 3), Is.EqualTo(2150m));
    }

    [Test]
    public void CumpleFranjaHoraria_TipoNormal_SiempreEsValido()
    {
        var tipo = TarjetaTipo.Normal;

        // Días de semana a cualquier hora
        Assert.That(tipo.CumpleFranjaHoraria(new DateTime(2026, 10, 19, 3, 0, 0)), Is.True);
        Assert.That(tipo.CumpleFranjaHoraria(new DateTime(2026, 10, 19, 14, 0, 0)), Is.True);
        Assert.That(tipo.CumpleFranjaHoraria(new DateTime(2026, 10, 19, 23, 30, 0)), Is.True);

        // Fines de semana
        Assert.That(tipo.CumpleFranjaHoraria(new DateTime(2026, 10, 24, 12, 0, 0)), Is.True); // Sábado
        Assert.That(tipo.CumpleFranjaHoraria(new DateTime(2026, 10, 25, 12, 0, 0)), Is.True); // Domingo
    }

    [Test]
    public void CumpleFranjaHoraria_Franquicias_ValidaDiasYHoras()
    {
        var tipo = TarjetaTipo.MedioBoleto;

        // Fines de semana no permitidos
        Assert.That(tipo.CumpleFranjaHoraria(new DateTime(2026, 10, 24, 12, 0, 0)), Is.False); // Sábado
        Assert.That(tipo.CumpleFranjaHoraria(new DateTime(2026, 10, 25, 12, 0, 0)), Is.False); // Domingo

        // Días de semana fuera de franja (06:00 a 22:00)
        Assert.That(tipo.CumpleFranjaHoraria(new DateTime(2026, 10, 19, 5, 59, 59)), Is.False);
        Assert.That(tipo.CumpleFranjaHoraria(new DateTime(2026, 10, 19, 22, 0, 1)), Is.False);

        // Límites exactos e interiores
        Assert.That(tipo.CumpleFranjaHoraria(new DateTime(2026, 10, 19, 6, 0, 0)), Is.True);
        Assert.That(tipo.CumpleFranjaHoraria(new DateTime(2026, 10, 19, 14, 30, 0)), Is.True);
        Assert.That(tipo.CumpleFranjaHoraria(new DateTime(2026, 10, 19, 22, 0, 0)), Is.True);
    }
}
