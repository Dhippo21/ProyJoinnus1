using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Notificacion
{
    public int IdNotificacion { get; set; }

    public int IdUsuario { get; set; }

    public string Titulo { get; set; } = null!;

    public string Mensaje { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public bool? Leida { get; set; }

    public DateTime? FechaEnvio { get; set; }

    public string? EnlaceDestino { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
