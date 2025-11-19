using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Entrada
{
    public int IdEntrada { get; set; }

    public int IdDetalle { get; set; }

    public string CodigoQr { get; set; } = null!;

    public string? Estado { get; set; }

    public DateTime? FechaEmision { get; set; }

    public DateTime? FechaUso { get; set; }

    public string? AsistenteNombre { get; set; }

    public string? AsistenteEmail { get; set; }

    public string? AsistenteDocumento { get; set; }

    public virtual DetalleCompra IdDetalleNavigation { get; set; } = null!;
}
