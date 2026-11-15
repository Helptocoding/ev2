namespace ControlGastos.Models;

public class DatosApp
{
    public List<Gasto> Gastos { get; set; } = new();
    public decimal PresupuestoMensual { get; set; }
}
