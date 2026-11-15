using System.Text.Json;
using System.Text.Json.Serialization;
using ControlGastos.Models;

namespace ControlGastos.Services;

public class GastoRepositorioArchivo : IGastoRepositorio
{
    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string rutaDelArchivo;
    private readonly SemaphoreSlim candado = new(1, 1);
    private DatosApp? datosEnMemoria;

    public GastoRepositorioArchivo(string rutaDelArchivo)
    {
        this.rutaDelArchivo = rutaDelArchivo;
    }

    public Task<IReadOnlyList<Gasto>> ObtenerTodosAsync()
        => EjecutarConCandadoAsync<IReadOnlyList<Gasto>>(datos =>
            Task.FromResult<IReadOnlyList<Gasto>>(
                datos.Gastos.Select(gasto => gasto.Clonar()).OrderByDescending(gasto => gasto.Fecha).ToList()));

    public Task GuardarAsync(Gasto gasto)
        => EjecutarConCandadoAsync(async datos =>
        {
            var posicion = datos.Gastos.FindIndex(existente => existente.Id == gasto.Id);

            if (posicion >= 0)
                datos.Gastos[posicion] = gasto.Clonar();
            else
                datos.Gastos.Add(gasto.Clonar());

            await PersistirAsync(datos);
            return true;
        });

    public Task<bool> EliminarAsync(Guid id)
        => EjecutarConCandadoAsync(async datos =>
        {
            var eliminados = datos.Gastos.RemoveAll(gasto => gasto.Id == id);

            if (eliminados > 0)
                await PersistirAsync(datos);

            return eliminados > 0;
        });

    public Task<decimal> ObtenerPresupuestoAsync()
        => EjecutarConCandadoAsync(datos => Task.FromResult(datos.PresupuestoMensual));

    public Task GuardarPresupuestoAsync(decimal presupuesto)
        => EjecutarConCandadoAsync(async datos =>
        {
            datos.PresupuestoMensual = presupuesto;
            await PersistirAsync(datos);
            return true;
        });

    private async Task<T> EjecutarConCandadoAsync<T>(Func<DatosApp, Task<T>> operacion)
    {
        await candado.WaitAsync();

        try
        {
            var datos = await CargarDatosAsync();
            return await operacion(datos);
        }
        finally
        {
            candado.Release();
        }
    }

    private async Task<DatosApp> CargarDatosAsync()
    {
        if (datosEnMemoria is not null)
            return datosEnMemoria;

        datosEnMemoria = await LeerDesdeArchivoAsync();
        return datosEnMemoria;
    }

    private async Task<DatosApp> LeerDesdeArchivoAsync()
    {
        if (!File.Exists(rutaDelArchivo))
            return new DatosApp();

        try
        {
            await using var flujo = File.OpenRead(rutaDelArchivo);
            return await JsonSerializer.DeserializeAsync<DatosApp>(flujo, OpcionesJson) ?? new DatosApp();
        }
        catch (JsonException)
        {
            return new DatosApp();
        }
    }

    private async Task PersistirAsync(DatosApp datos)
    {
        var carpeta = Path.GetDirectoryName(rutaDelArchivo);

        if (!string.IsNullOrEmpty(carpeta))
            Directory.CreateDirectory(carpeta);

        await using var flujo = File.Create(rutaDelArchivo);
        await JsonSerializer.SerializeAsync(flujo, datos, OpcionesJson);
    }
}
