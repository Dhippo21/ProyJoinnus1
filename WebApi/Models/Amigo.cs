using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Amigo
{
    public int IdRelacion { get; set; }

    public int IdUsuario { get; set; }

    public int IdAmigo { get; set; }

    public DateTime? FechaAgregado { get; set; }

    public string? Estado { get; set; }

    public virtual Usuario IdAmigoNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
