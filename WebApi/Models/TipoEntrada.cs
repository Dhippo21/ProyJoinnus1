using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class TipoEntrada
{
    public int IdTipoEntrada { get; set; }

    public int IdEvento { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal Precio { get; set; }

    public int CupoTotal { get; set; }

    public int CupoDisponible { get; set; }

    public DateTime? FechaInicioVenta { get; set; }

    public DateTime? FechaFinVenta { get; set; }

    public int? MinimoCompra { get; set; }

    public int? MaximoCompra { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<DetalleCompra> DetalleCompras { get; set; } = new List<DetalleCompra>();

    public virtual Evento IdEventoNavigation { get; set; } = null!;

    public virtual ICollection<Promocion> Promocions { get; set; } = new List<Promocion>();
}
