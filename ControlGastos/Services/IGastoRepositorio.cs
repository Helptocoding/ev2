using ControlGastos.Models;

namespace ControlGastos.Services;

public interface IGastoRepositorio
{
    Task<IReadOnlyList<Gasto>> ObtenerTodosAsync();
    Task GuardarAsync(Gasto gasto);
    Task<bool> EliminarAsync(Guid id);
    Task<decimal> ObtenerPresupuestoAsync();
    Task GuardarPresupuestoAsync(decimal presupuesto);
}
