using ControlGastos.Models;
using ControlGastos.Services;

namespace ControlGastos.Plataforma;

public class ServicioNavegacion : IServicioNavegacion
{
    public const string RutaFormularioDeGasto = "formulariogasto";
    public const string ParametroGasto = "gasto";

    public Task IrAFormularioDeGastoAsync(Gasto? gastoAEditar)
    {
        if (gastoAEditar is null)
            return Shell.Current.GoToAsync(RutaFormularioDeGasto);

        var parametros = new ShellNavigationQueryParameters
        {
            { ParametroGasto, gastoAEditar.Clonar() }
        };

        return Shell.Current.GoToAsync(RutaFormularioDeGasto, parametros);
    }

    public Task VolverAsync() => Shell.Current.GoToAsync("..");
}
