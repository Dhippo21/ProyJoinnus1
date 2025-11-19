using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string CorreoElectronico { get; set; } = null!;

    public string ContrasenaHash { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string? Pais { get; set; }

    public string? Ciudad { get; set; }

    public string? TipoDocumento { get; set; }

    public string? NumeroDocumento { get; set; }

    public string? Genero { get; set; }

    public string? Telefono { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool? VerificadoEmail { get; set; }

    public bool? Bloqueado { get; set; }

    public int? IntentosFallidos { get; set; }

    public DateTime? UltimoIntentoFallido { get; set; }

    public string? RestablecerToken { get; set; }

    public DateTime? RestablecerExpira { get; set; }

    public bool? AutenticacionDosPasos { get; set; }

    public string? MetodoVerificacionPreferido { get; set; }

    public bool? NotificarInicioSesion { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<Amigo> AmigoIdAmigoNavigations { get; set; } = new List<Amigo>();

    public virtual ICollection<Amigo> AmigoIdUsuarioNavigations { get; set; } = new List<Amigo>();

    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();

    public virtual ICollection<Evento> Eventos { get; set; } = new List<Evento>();

    public virtual ICollection<HistorialBusqueda> HistorialBusqueda { get; set; } = new List<HistorialBusqueda>();

    public virtual ICollection<Notificacion> Notificacions { get; set; } = new List<Notificacion>();

    public virtual ICollection<Sesion> Sesions { get; set; } = new List<Sesion>();

    public virtual ICollection<UsuarioRol> UsuarioRols { get; set; } = new List<UsuarioRol>();
}
