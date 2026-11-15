using ControlGastos.Models;

namespace ControlGastos.Helpers;

public static class CalculadoraGastos
{
    public const decimal UmbralDeAdvertencia = 0.80m;

    public static decimal CalcularTotal(IEnumerable<Gasto> gastos)
        => gastos.Sum(gasto => gasto.Monto);

    public static IEnumerable<Gasto> FiltrarPorMes(IEnumerable<Gasto> gastos, int anio, int mes)
        => gastos.Where(gasto => gasto.Fecha.Year == anio && gasto.Fecha.Month == mes);

    public static decimal CalcularPorcentaje(decimal parte, decimal total)
    {
        if (total <= 0)
            return 0;

        return Math.Round(parte / total * 100m, 1);
    }

    public static IReadOnlyList<ResumenCategoria> AgruparPorCategoria(IEnumerable<Gasto> gastos)
    {
        var listaDeGastos = gastos.ToList();
        var totalGeneral = CalcularTotal(listaDeGastos);

        return listaDeGastos
            .GroupBy(gasto => gasto.Categoria)
            .Select(grupo => new ResumenCategoria
            {
                Categoria = grupo.Key,
                Total = grupo.Sum(gasto => gasto.Monto),
                CantidadDeGastos = grupo.Count(),
                Porcentaje = CalcularPorcentaje(grupo.Sum(gasto => gasto.Monto), totalGeneral)
            })
            .OrderByDescending(resumen => resumen.Total)
            .ToList();
    }

    public static CategoriaGasto? ObtenerCategoriaConMayorGasto(IEnumerable<Gasto> gastos)
        => AgruparPorCategoria(gastos).FirstOrDefault()?.Categoria;

    public static int ObtenerDiasTranscurridos(DateTime mes, DateTime hoy)
    {
        var esElMesActual = mes.Year == hoy.Year && mes.Month == hoy.Month;

        return esElMesActual
            ? hoy.Day
            : DateTime.DaysInMonth(mes.Year, mes.Month);
    }

    public static decimal CalcularPromedioDiario(decimal totalGastado, int diasTranscurridos)
    {
        if (diasTranscurridos <= 0)
            return 0;

        return Math.Round(totalGastado / diasTranscurridos, 2);
    }

    public static decimal CalcularRestante(decimal presupuesto, decimal gastado)
        => presupuesto - gastado;

    public static double CalcularProgreso(decimal presupuesto, decimal gastado)
    {
        if (presupuesto <= 0)
            return 0;

        var progreso = (double)(gastado / presupuesto);
        return Math.Clamp(progreso, 0d, 1d);
    }

    public static EstadoPresupuesto ObtenerEstadoPresupuesto(decimal presupuesto, decimal gastado)
    {
        if (presupuesto <= 0)
            return EstadoPresupuesto.SinPresupuesto;

        if (gastado > presupuesto)
            return EstadoPresupuesto.LimiteExcedido;

        if (gastado >= presupuesto * UmbralDeAdvertencia)
            return EstadoPresupuesto.CercaDelLimite;

        return EstadoPresupuesto.DentroDelLimite;
    }
}
