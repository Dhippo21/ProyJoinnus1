using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class DetalleCompra
{
    public int IdDetalle { get; set; }

    public int IdCompra { get; set; }

    public int IdTipoEntrada { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal? Subtotal { get; set; }

    public virtual ICollection<Entrada> Entrada { get; set; } = new List<Entrada>();

    public virtual Compra IdCompraNavigation { get; set; } = null!;

    public virtual TipoEntrada IdTipoEntradaNavigation { get; set; } = null!;
}
