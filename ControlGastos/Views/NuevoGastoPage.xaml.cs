using ControlGastos.Models;
using ControlGastos.Plataforma;
using ControlGastos.ViewModels;

namespace ControlGastos.Views;

public partial class NuevoGastoPage : ContentPage, IQueryAttributable
{
    private readonly VMNuevoGasto viewModel;

    public NuevoGastoPage(VMNuevoGasto viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> parametros)
    {
        if (parametros.TryGetValue(ServicioNavegacion.ParametroGasto, out var valor) && valor is Gasto gasto)
            viewModel.Cargar(gasto);
    }
}
