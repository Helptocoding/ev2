using ControlGastos.Models;
using ControlGastos.ViewModels;

namespace ControlGastos.Views;

public partial class GastosPage : ContentPage
{
    private readonly VMGastos viewModel;

    public GastosPage(VMGastos viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.CargarAsync();
    }

    private void AlEditarDesdeDeslizamiento(object? sender, EventArgs e)
    {
        if (ObtenerGastoDe(sender) is { } gasto)
            viewModel.EditarGastoCommand.Execute(gasto);
    }

    private void AlEliminarDesdeDeslizamiento(object? sender, EventArgs e)
    {
        if (ObtenerGastoDe(sender) is { } gasto)
            viewModel.EliminarGastoCommand.Execute(gasto);
    }

    private static Gasto? ObtenerGastoDe(object? origen)
        => (origen as BindableObject)?.BindingContext as Gasto;
}
