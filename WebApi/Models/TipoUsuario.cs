using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class TipoUsuario
{
    public int IdTipoUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<UsuarioRol> UsuarioRols { get; set; } = new List<UsuarioRol>();
}
