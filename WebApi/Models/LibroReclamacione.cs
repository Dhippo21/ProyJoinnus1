using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class LibroReclamacione
{
    public int IdReclamo { get; set; }

    public int? IdUsuario { get; set; }

    public string TipoDocumento { get; set; } = null!;

    public string NumeroDocumento { get; set; } = null!;

    public string NombreCompleto { get; set; } = null!;

    public string? CorreoContacto { get; set; }

    public string? TelefonoContacto { get; set; }

    public string TipoReclamo { get; set; } = null!;

    public string Detalle { get; set; } = null!;

    public string? AdjuntosUrls { get; set; }

    public DateTime? FechaReclamo { get; set; }

    public string NumeroRegistro { get; set; } = null!;

    public string? Estado { get; set; }

    public DateTime? FechaRespuesta { get; set; }

    public string? Respuesta { get; set; }
}
