using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class HistorialBusqueda
{
    public int IdBusqueda { get; set; }

    public int? IdUsuario { get; set; }

    public string? Termino { get; set; }

    public int? IdCategoria { get; set; }

    public int? IdCiudad { get; set; }

    public DateTime? FechaBusqueda { get; set; }

    public int? ResultadosMostrados { get; set; }

    public virtual Categoria? IdCategoriaNavigation { get; set; }

    public virtual Ciudad? IdCiudadNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
