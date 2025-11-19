using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class UsuarioRol
{
    public int IdUsuarioRol { get; set; }

    public int IdUsuario { get; set; }

    public int IdTipoUsuario { get; set; }

    public DateTime? FechaAsignacion { get; set; }

    public bool? Activo { get; set; }

    public virtual TipoUsuario IdTipoUsuarioNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
