using ControlGastos.ViewModels;

namespace ControlGastos.Views;

public partial class ResumenPage : ContentPage
{
    private readonly VMResumen viewModel;

    public ResumenPage(VMResumen viewModel)
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
}
