using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Ciudad
{
    public int IdCiudad { get; set; }

    public string Nombre { get; set; } = null!;

    public string Pais { get; set; } = null!;

    public bool? Activo { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<HistorialBusqueda> HistorialBusqueda { get; set; } = new List<HistorialBusqueda>();

    public virtual ICollection<Lugar> Lugar { get; set; } = new List<Lugar>();
}
