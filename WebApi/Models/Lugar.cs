using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Lugar
{
    public int IdLugar { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Direccion { get; set; }

    public int IdCiudad { get; set; }

    public decimal? Latitud { get; set; }

    public decimal? Longitud { get; set; }

    public int? CapacidadMaxima { get; set; }

    public string? SitioWeb { get; set; }

    public string? Telefono { get; set; }

    public bool? Activo { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<Evento> Eventos { get; set; } = new List<Evento>();

    public virtual Ciudad IdCiudadNavigation { get; set; } = null!;
}
