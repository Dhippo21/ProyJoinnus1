using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Faq
{
    public int IdPregunta { get; set; }

    public string Categoria { get; set; } = null!;

    public string Pregunta { get; set; } = null!;

    public string Respuesta { get; set; } = null!;

    public int? Orden { get; set; }

    public bool? Activo { get; set; }

    public DateTime? FechaCreacion { get; set; }
}
