using ControlGastos.Plataforma;
using ControlGastos.Services;
using ControlGastos.ViewModels;
using ControlGastos.Views;

namespace ControlGastos;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        RegistrarServicios(builder.Services);
        RegistrarViewModels(builder.Services);
        RegistrarPaginas(builder.Services);

        return builder.Build();
    }

    private static void RegistrarServicios(IServiceCollection servicios)
    {
        var rutaDelArchivo = Path.Combine(FileSystem.AppDataDirectory, "gastos.json");

        servicios.AddSingleton<IGastoRepositorio>(_ => new GastoRepositorioArchivo(rutaDelArchivo));
        servicios.AddSingleton<IServicioDispositivo, ServicioDispositivo>();
        servicios.AddSingleton<IServicioNavegacion, ServicioNavegacion>();
        servicios.AddSingleton<ContextoMes>();
    }

    private static void RegistrarViewModels(IServiceCollection servicios)
    {
        servicios.AddTransient<VMGastos>();
        servicios.AddTransient<VMNuevoGasto>();
        servicios.AddTransient<VMResumen>();
    }

    private static void RegistrarPaginas(IServiceCollection servicios)
    {
        servicios.AddTransient<GastosPage>();
        servicios.AddTransient<NuevoGastoPage>();
        servicios.AddTransient<ResumenPage>();
    }
}
