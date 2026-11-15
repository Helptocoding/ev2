namespace ControlGastos.Models;

public class Gasto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Descripcion { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public CategoriaGasto Categoria { get; set; } = CategoriaGasto.Otros;
    public DateTime Fecha { get; set; } = DateTime.Today;

    public Gasto Clonar() => new()
    {
        Id = Id,
        Descripcion = Descripcion,
        Monto = Monto,
        Categoria = Categoria,
        Fecha = Fecha
    };
}
