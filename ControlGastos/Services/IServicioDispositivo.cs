namespace ControlGastos.Services;

public interface IServicioDispositivo
{
    Task MostrarMensajeAsync(string titulo, string mensaje);
    Task<bool> ConfirmarAsync(string titulo, string mensaje);
    Task<string?> PedirNumeroAsync(string titulo, string mensaje, string valorInicial);
    void VibrarSuavemente();
}
