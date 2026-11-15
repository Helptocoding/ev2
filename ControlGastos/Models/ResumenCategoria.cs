namespace ControlGastos.Models;

public class ResumenCategoria
{
    public CategoriaGasto Categoria { get; init; }
    public decimal Total { get; init; }
    public int CantidadDeGastos { get; init; }
    public decimal Porcentaje { get; init; }

    public double ProgresoParaBarra => (double)(Porcentaje / 100m);
}
