using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Sesion
{
    public int IdSesion { get; set; }

    public int IdUsuario { get; set; }

    public string Token { get; set; } = null!;

    public DateTime FechaInicio { get; set; }

    public DateTime FechaExpiracion { get; set; }

    public string? NroIp { get; set; }

    public string? Dispositivo { get; set; }

    public bool? Activa { get; set; }

    public bool? CerradaPorUsuario { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
