using ControlGastos.Plataforma;
using ControlGastos.Views;

namespace ControlGastos;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(ServicioNavegacion.RutaFormularioDeGasto, typeof(NuevoGastoPage));
    }
}
