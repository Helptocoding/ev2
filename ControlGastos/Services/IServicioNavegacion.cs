using ControlGastos.Models;

namespace ControlGastos.Services;

public interface IServicioNavegacion
{
    Task IrAFormularioDeGastoAsync(Gasto? gastoAEditar);
    Task VolverAsync();
}
