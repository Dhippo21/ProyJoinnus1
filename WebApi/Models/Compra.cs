using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Compra
{
    public int IdCompra { get; set; }

    public int IdUsuario { get; set; }

    public int IdEvento { get; set; }

    public DateTime? FechaCompra { get; set; }

    public string MetodoPago { get; set; } = null!;

    public string EstadoPago { get; set; } = null!;

    public decimal MontoTotal { get; set; }

    public string? CodigoTransaccion { get; set; }

    public bool? Confirmada { get; set; }

    public bool? Reembolsado { get; set; }

    public virtual ICollection<DetalleCompra> DetalleCompras { get; set; } = new List<DetalleCompra>();

    public virtual Evento IdEventoNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
