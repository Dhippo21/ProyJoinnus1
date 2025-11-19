using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Categoria
{
    public int IdCategoria { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? ImagenUrl { get; set; }

    public bool? Activo { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<Evento> Eventos { get; set; } = new List<Evento>();

    public virtual ICollection<HistorialBusqueda> HistorialBusqueda { get; set; } = new List<HistorialBusqueda>();
}
