using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class PoliticaLegal
{
    public int IdPolitica { get; set; }

    public string Tipo { get; set; } = null!;

    public string Contenido { get; set; } = null!;

    public DateTime? FechaActualizacion { get; set; }

    public string? Version { get; set; }
}
