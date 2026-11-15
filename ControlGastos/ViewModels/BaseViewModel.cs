using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ControlGastos.ViewModels;

public abstract class BaseViewModel : INotifyPropertyChanged
{
    private string titulo = string.Empty;
    private bool estaOcupado;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Titulo
    {
        get => titulo;
        set => AsignarPropiedad(ref titulo, value);
    }

    public bool EstaOcupado
    {
        get => estaOcupado;
        private set
        {
            if (AsignarPropiedad(ref estaOcupado, value))
                NotificarCambio(nameof(NoEstaOcupado));
        }
    }

    public bool NoEstaOcupado => !EstaOcupado;

    protected bool AsignarPropiedad<T>(ref T campo, T valor, [CallerMemberName] string? nombrePropiedad = null)
    {
        if (EqualityComparer<T>.Default.Equals(campo, valor))
            return false;

        campo = valor;
        NotificarCambio(nombrePropiedad);
        return true;
    }

    protected void NotificarCambio([CallerMemberName] string? nombrePropiedad = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));

    protected async Task EjecutarOcupadoAsync(Func<Task> operacion)
    {
        if (EstaOcupado)
            return;

        try
        {
            EstaOcupado = true;
            await operacion();
        }
        catch (Exception excepcion)
        {
            await AlOcurrirErrorAsync(excepcion);
        }
        finally
        {
            EstaOcupado = false;
        }
    }

    protected virtual Task AlOcurrirErrorAsync(Exception excepcion) => Task.CompletedTask;
}
