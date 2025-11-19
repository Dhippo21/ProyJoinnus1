using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Promocion
{
    public int IdPromocion { get; set; }

    public string Codigo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string TipoDescuento { get; set; } = null!;

    public decimal ValorDescuento { get; set; }

    public int? IdEvento { get; set; }

    public int? IdTipoEntrada { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public int? UsoMaximo { get; set; }

    public int? UsoActual { get; set; }

    public bool? Activo { get; set; }

    public virtual Evento? IdEventoNavigation { get; set; }

    public virtual TipoEntrada? IdTipoEntradaNavigation { get; set; }
}
