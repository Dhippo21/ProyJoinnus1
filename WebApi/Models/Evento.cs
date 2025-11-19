using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Evento
{
    public int IdEvento { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int IdCategoria { get; set; }

    public int IdLugar { get; set; }

    public int IdOrganizador { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public TimeOnly? HoraEvento { get; set; }

    public string? ImagenPortada { get; set; }

    public string? ImagenBanner { get; set; }

    public string? UrlAmigable { get; set; }

    public bool? Activo { get; set; }

    public bool? Cancelado { get; set; }

    public bool? Destacado { get; set; }

    public bool? EnTendencia { get; set; }

    public string? PoliticaReembolso { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();

    public virtual Categoria IdCategoriaNavigation { get; set; } = null!;

    public virtual Lugar IdLugarNavigation { get; set; } = null!;

    public virtual Usuario IdOrganizadorNavigation { get; set; } = null!;

    public virtual ICollection<Promocion> Promocions { get; set; } = new List<Promocion>();

    public virtual ICollection<TipoEntrada> TipoEntrada { get; set; } = new List<TipoEntrada>();
}
