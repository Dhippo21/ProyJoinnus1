using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WebApi.Models;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/usuario")]
    public class UsuarioController : ControllerBase
    {
        private readonly dbJoinnusContext _context;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(dbJoinnusContext context, ILogger<UsuarioController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Método para verificar si el usuario está autenticado mediante token
        private async Task<Usuario> GetAuthenticatedUser()
        {
            // Obtener el token del encabezado de autorización
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            // Buscar la sesión activa con ese token
            var sesion = await _context.Sesions
                .Include(s => s.IdUsuarioNavigation)
                .FirstOrDefaultAsync(s => s.Token == token && s.Activa == true && s.FechaExpiracion > DateTime.Now);

            return sesion?.IdUsuarioNavigation;
        }

        //Servicio 5 - Cambiar Contraseña
        //===============================================================================
        [HttpPost("Cambiar-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                // Verificar que el usuario esté autenticado
                var usuario = await GetAuthenticatedUser();
                if (usuario == null)
                {
                    return Unauthorized(new { message = "No autorizado" });
                }

                // Validar que se proporcionaron todos los campos
                if (string.IsNullOrEmpty(request.ContrasenaActual) ||
                    string.IsNullOrEmpty(request.NuevaContrasena) ||
                    string.IsNullOrEmpty(request.ConfirmacionContrasena))
                {
                    return BadRequest(new { message = "Todos los campos son obligatorios" });
                }

                // Validar que las contraseñas nuevas coincidan
                if (request.NuevaContrasena != request.ConfirmacionContrasena)
                {
                    return BadRequest(new { message = "Las contraseñas nuevas no coinciden" });
                }

                // Validar la nueva contraseña
                if (!IsValidPassword(request.NuevaContrasena))
                {
                    return BadRequest(new { message = "La contraseña debe tener al menos 8 caracteres, incluyendo mayúsculas, minúsculas, números y caracteres especiales" });
                }

                // Verificar la contraseña actual
                if (!VerifyPassword(request.ContrasenaActual, usuario.ContrasenaHash))
                {
                    return BadRequest(new { message = "La contraseña actual es incorrecta" });
                }

                // Verificar que la nueva contraseña sea diferente a la actual
                if (VerifyPassword(request.NuevaContrasena, usuario.ContrasenaHash))
                {
                    return BadRequest(new { message = "La nueva contraseña debe ser diferente a la actual" });
                }

                // Actualizar la contraseña
                usuario.ContrasenaHash = HashPassword(request.NuevaContrasena);
                await _context.SaveChangesAsync();

                // Enviar notificación de cambio de contraseña (simulado)
                _logger.LogInformation($"Contraseña actualizada para el usuario {usuario.CorreoElectronico}");

                return Ok(new { message = "Contraseña actualizada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar la contraseña");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        private bool IsValidPassword(string password)
        {
            // Mínimo 8 caracteres, al menos una mayúscula, una minúscula, un número y un carácter especial
            var regex = new System.Text.RegularExpressions.Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
            return regex.IsMatch(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            try
            {
                // Implementar la verificación de contraseña
                using (var sha256 = SHA256.Create())
                {
                    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    var hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

                    return hashedPassword == hash.ToLower();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar contraseña");
                return false;
            }
        }

        private string HashPassword(string password)
        {
            // Implementar el hashing de contraseña
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public class ChangePasswordRequest
        {
            public string ContrasenaActual { get; set; }
            public string NuevaContrasena { get; set; }
            public string ConfirmacionContrasena { get; set; }
        }

        //Servicio 6 - Solicitar Activación Autenticación en 2 Pasos
        //===============================================================================
        [HttpPost("Solicitar-Activacion-Autenticación-2fa")]  //request-2fa
        [Authorize]
        public async Task<IActionResult> RequestTwoFactorAuthentication()
        {
            try
            {
                // Verificar que el usuario esté autenticado
                var usuario = await GetAuthenticatedUser();
                if (usuario == null)
                {
                    return Unauthorized(new { message = "No autorizado" });
                }

                // Verificar que la cuenta esté activa
                if (usuario.Activo != true)
                {
                    return BadRequest(new { message = "La cuenta no está activa" });
                }

                // Verificar que no tenga ya activada la autenticación en 2 pasos
                if (usuario.AutenticacionDosPasos == true)
                {
                    return BadRequest(new { message = "La autenticación en dos pasos ya está activada" });
                }

                // Generar un código de verificación de 6 dígitos
                var codigoVerificacion = GenerateVerificationCode();

                // Guardar el código en la base de datos (usamos temporalmente RestablecerToken)
                usuario.RestablecerToken = codigoVerificacion;
                usuario.RestablecerExpira = DateTime.Now.AddMinutes(10); // Código válido por 10 minutos
                await _context.SaveChangesAsync();

                // Enviar el código al correo electrónico (simulado)
                _logger.LogInformation($"Código de verificación enviado a {usuario.CorreoElectronico}: {codigoVerificacion}");

                return Ok(new
                {
                    message = "Hemos enviado un código al correo que tienes registrado en Joinnus. Introdúcelo aquí para activar la autenticación en dos pasos.",
                    emailMasked = MaskEmail(usuario.CorreoElectronico)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al solicitar activación de autenticación en dos pasos");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        private string GenerateVerificationCode()
        {
            // Generar un código de 6 dígitos
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return string.Empty;

            var parts = email.Split('@');
            if (parts.Length != 2)
                return email;

            var username = parts[0];
            var domain = parts[1];

            // Mostrar solo los primeros 2 caracteres y el último del nombre de usuario
            var maskedUsername = username.Length <= 2
                ? username
                : username.Substring(0, 2) + new string('*', username.Length - 3) + username.Substring(username.Length - 1);

            return $"{maskedUsername}@{domain}";
        }

        //Servicio 7 - Activar Autenticación en 2 Pasos
        //===============================================================================
        [HttpPost("Activar-Autenticacion-2Pasos")] //activate-2fa
        [Authorize]
        public async Task<IActionResult> ActivateTwoFactorAuthentication([FromBody] ActivateTwoFactorRequest request)
        {
            try
            {
                // Verificar que el usuario esté autenticado
                var usuario = await GetAuthenticatedUser();
                if (usuario == null)
                {
                    return Unauthorized(new { message = "No autorizado" });
                }

                // Validar que se proporcionó el código
                if (string.IsNullOrEmpty(request.CodigoVerificacion))
                {
                    return BadRequest(new { message = "El código de verificación es requerido" });
                }

                // Verificar que el código coincida y no haya expirado
                if (usuario.RestablecerToken != request.CodigoVerificacion ||
                    usuario.RestablecerExpira < DateTime.Now)
                {
                    return BadRequest(new { message = "Código de verificación inválido o expirado" });
                }

                // Activar la autenticación en dos pasos
                usuario.AutenticacionDosPasos = true;
                usuario.MetodoVerificacionPreferido = "Email"; // Por defecto, correo electrónico
                usuario.RestablecerToken = null;
                usuario.RestablecerExpira = null;
                await _context.SaveChangesAsync();

                // Enviar notificación de activación (simulado)
                _logger.LogInformation($"Autenticación en dos pasos activada para el usuario {usuario.CorreoElectronico}");

                return Ok(new { message = "La autenticación en dos pasos ha sido activada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al activar autenticación en dos pasos");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        public class ActivateTwoFactorRequest
        {
            public string CodigoVerificacion { get; set; }
        }

        //Servicio 8 - Elegir Método de Verificación
        //===============================================================================
        [HttpPost("Elegir-Metodo-Verificacion")] //7set-verification-method
        [Authorize]
        public async Task<IActionResult> SetVerificationMethod([FromBody] SetVerificationMethodRequest request)
        {
            try
            {
                // Verificar que el usuario esté autenticado
                var usuario = await GetAuthenticatedUser();
                if (usuario == null)
                {
                    return Unauthorized(new { message = "No autorizado" });
                }

                // Verificar que tenga activada la autenticación en dos pasos
                if (usuario.AutenticacionDosPasos != true)
                {
                    return BadRequest(new { message = "La autenticación en dos pasos no está activada" });
                }

                // Validar que se haya elegido al menos un método de verificación
                if (request.MetodosVerificacion == null || request.MetodosVerificacion.Count == 0)
                {
                    return BadRequest(new { message = "Debe seleccionar al menos un método de verificación" });
                }

                // Validar que los métodos sean válidos
                var metodosValidos = new List<string> { "Email", "EmailAlternativo", "SMS" };
                foreach (var metodo in request.MetodosVerificacion)
                {
                    if (!metodosValidos.Contains(metodo))
                    {
                        return BadRequest(new { message = $"Método de verificación inválido: {metodo}" });
                    }
                }

                // Guardar el método preferido (el primero de la lista)
                usuario.MetodoVerificacionPreferido = request.MetodosVerificacion[0];

                // Guardar cuándo mostrar la verificación
                usuario.NotificarInicioSesion = request.MostrarAlIniciarSesion;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Método de verificación actualizado exitosamente",
                    metodoPreferido = usuario.MetodoVerificacionPreferido,
                    mostrarAlIniciarSesion = usuario.NotificarInicioSesion
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al establecer método de verificación");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        public class SetVerificationMethodRequest
        {
            public List<string> MetodosVerificacion { get; set; }
            public bool MostrarAlIniciarSesion { get; set; }
            public bool MostrarAlVisualizarEntrada { get; set; }
        }

        //Servicio 9 - Visualizar Mis Entradas
        //===============================================================================
        [HttpGet("Visualizar-Mis-Entradas")]
        [Authorize]
        public async Task<IActionResult> GetTickets()
        {
            try
            {
                _logger.LogInformation("Iniciando GetTickets");

                // Obtener el usuario del contexto (establecido por el middleware)
                if (!HttpContext.Items.ContainsKey("User"))
                {
                    _logger.LogWarning("Usuario no encontrado en el contexto");
                    return Unauthorized(new { message = "No autorizado" });
                }

                var usuario = (Usuario)HttpContext.Items["User"];
                _logger.LogInformation($"Usuario encontrado en el contexto: {usuario.CorreoElectronico} (ID: {usuario.IdUsuario})");

                // Obtener las compras del usuario
                var compras = await _context.Compras
                    .Include(c => c.IdEventoNavigation)
                    .Include(c => c.DetalleCompras)
                        .ThenInclude(dc => dc.IdTipoEntradaNavigation)
                    .Include(c => c.DetalleCompras)
                        .ThenInclude(dc => dc.Entrada)
                    .Where(c => c.IdUsuario == usuario.IdUsuario)
                    .OrderByDescending(c => c.FechaCompra)
                    .ToListAsync();

                // Transformar los datos a un formato más amigable
                var tickets = compras.SelectMany(c => c.DetalleCompras.Select(dc => new
                {
                    IdCompra = c.IdCompra,
                    FechaCompra = c.FechaCompra,
                    Evento = new
                    {
                        IdEvento = c.IdEventoNavigation.IdEvento,
                        Titulo = c.IdEventoNavigation.Titulo,
                        FechaInicio = c.IdEventoNavigation.FechaInicio,
                        ImagenPortada = c.IdEventoNavigation.ImagenPortada
                    },
                    TipoEntrada = new
                    {
                        IdTipoEntrada = dc.IdTipoEntradaNavigation.IdTipoEntrada,
                        Nombre = dc.IdTipoEntradaNavigation.Nombre,
                        Precio = dc.IdTipoEntradaNavigation.Precio
                    },
                    Cantidad = dc.Cantidad,
                    Subtotal = dc.Subtotal,
                    Entradas = dc.Entrada.Select(e => new
                    {
                        IdEntrada = e.IdEntrada,
                        CodigoQR = e.CodigoQr,
                        Estado = e.Estado,
                        FechaEmision = e.FechaEmision,
                        FechaUso = e.FechaUso
                    }).ToList()
                })).ToList();

                _logger.LogInformation($"Se encontraron {tickets.Count} entradas para el usuario {usuario.CorreoElectronico}");
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las entradas del usuario");

                // En desarrollo, devolver más detalles del error
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    return StatusCode(500, new
                    {
                        message = "Error interno del servidor",
                        exceptionMessage = ex.Message,
                        stackTrace = ex.StackTrace,
                        innerException = ex.InnerException?.Message
                    });
                }

                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        //Servicio 10 - Visualizar Información Personal
        //===============================================================================
        [HttpGet("Visualizar-Informacion-Personal")] //profile
        // Quitamos temporalmente el atributo [Authorize] para probar
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                _logger.LogInformation("Iniciando GetProfile");

                // Obtener el usuario del contexto (establecido por el middleware)
                if (!HttpContext.Items.ContainsKey("User"))
                {
                    _logger.LogWarning("Usuario no encontrado en el contexto");
                    return Unauthorized(new { message = "No autorizado" });
                }

                var usuario = (Usuario)HttpContext.Items["User"];
                _logger.LogInformation($"Usuario encontrado en el contexto: {usuario.CorreoElectronico}");

                // Obtener los roles del usuario
                var roles = await _context.UsuarioRols
                    .Include(ur => ur.IdTipoUsuarioNavigation)
                    .Where(ur => ur.IdUsuario == usuario.IdUsuario && ur.Activo == true)
                    .Select(ur => ur.IdTipoUsuarioNavigation.Nombre)
                    .ToListAsync();

                // Crear el objeto de respuesta
                var profile = new
                {
                    IdUsuario = usuario.IdUsuario,
                    CorreoElectronico = usuario.CorreoElectronico,
                    Nombre = usuario.Nombre,
                    Apellidos = usuario.Apellidos,
                    NombreCompleto = $"{usuario.Nombre} {usuario.Apellidos}",
                    Pais = usuario.Pais,
                    Ciudad = usuario.Ciudad,
                    TipoDocumento = usuario.TipoDocumento,
                    NumeroDocumento = usuario.NumeroDocumento,
                    Genero = usuario.Genero,
                    Telefono = usuario.Telefono,
                    FechaNacimiento = usuario.FechaNacimiento,
                    FechaRegistro = usuario.FechaRegistro,
                    VerificadoEmail = usuario.VerificadoEmail,
                    AutenticacionDosPasos = usuario.AutenticacionDosPasos,
                    MetodoVerificacionPreferido = usuario.MetodoVerificacionPreferido,
                    NotificarInicioSesion = usuario.NotificarInicioSesion,
                    Roles = roles
                };

                _logger.LogInformation($"Perfil generado para {usuario.CorreoElectronico}");
                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el perfil del usuario");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        //Servicio 11 - Visualizar Lista de Amigos
        //===============================================================================
        [HttpGet("Visualizar-Lista-Amigos")]
        public async Task<IActionResult> GetFriends()
        {
            try
            {
                _logger.LogInformation("Iniciando GetFriends");

                // Obtener el usuario del contexto (establecido por el middleware)
                if (!HttpContext.Items.ContainsKey("User"))
                {
                    _logger.LogWarning("Usuario no encontrado en el contexto");
                    return Unauthorized(new { message = "No autorizado" });
                }

                var usuario = (Usuario)HttpContext.Items["User"];
                _logger.LogInformation($"Usuario encontrado en el contexto: {usuario.CorreoElectronico} (ID: {usuario.IdUsuario})");

                // Verificar si el usuario existe en la base de datos
                var usuarioDB = await _context.Usuarios.FindAsync(usuario.IdUsuario);
                if (usuarioDB == null)
                {
                    _logger.LogWarning($"Usuario con ID {usuario.IdUsuario} no encontrado en la base de datos");
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                _logger.LogInformation($"Buscando amigos para el usuario {usuario.CorreoElectronico}");

                // Obtener la lista de amigos
                var amigosQuery = await _context.Amigos
                    .Include(a => a.IdAmigoNavigation)
                    .Where(a => a.IdUsuario == usuario.IdUsuario && a.Estado == "Activo")
                    .Select(a => new
                    {
                        IdAmigo = a.IdAmigo,
                        Nombre = a.IdAmigoNavigation.Nombre,
                        Apellidos = a.IdAmigoNavigation.Apellidos,
                        CorreoElectronico = a.IdAmigoNavigation.CorreoElectronico,
                        Pais = a.IdAmigoNavigation.Pais,
                        Ciudad = a.IdAmigoNavigation.Ciudad,
                        FechaAgregado = a.FechaAgregado
                    })
                    .ToListAsync();

                // Crear el nombre completo en memoria (no en la consulta SQL)
                var amigos = amigosQuery.Select(a => new
                {
                    a.IdAmigo,
                    a.Nombre,
                    a.Apellidos,
                    NombreCompleto = $"{a.Nombre} {a.Apellidos}",
                    a.CorreoElectronico,
                    a.Pais,
                    a.Ciudad,
                    a.FechaAgregado
                })
                .OrderBy(a => a.NombreCompleto)
                .ToList();

                _logger.LogInformation($"Se encontraron {amigos.Count} amigos para el usuario {usuario.CorreoElectronico}");
                return Ok(amigos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de amigos");

                // En desarrollo, devolver más detalles del error
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    return StatusCode(500, new
                    {
                        message = "Error interno del servidor",
                        exceptionMessage = ex.Message,
                        stackTrace = ex.StackTrace,
                        innerException = ex.InnerException?.Message
                    });
                }

                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        //Servicio 12 - Cerrar Sesión
        //===============================================================================
        [HttpPost("Cerrar-Sesion")] //logout
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            try
            {
                // Obtener el token del encabezado de autorización
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new { message = "Token no proporcionado" });
                }

                if (request.CerrarEnTodos)
                {
                    // Cerrar todas las sesiones del usuario
                    var sesiones = await _context.Sesions
                        .Where(s => s.Token == token)
                        .ToListAsync();

                    if (sesiones.Any())
                    {
                        var idUsuario = sesiones.First().IdUsuario;
                        var todasLasSesiones = await _context.Sesions
                            .Where(s => s.IdUsuario == idUsuario && s.Activa == true)
                            .ToListAsync();

                        foreach (var sesion in todasLasSesiones)
                        {
                            sesion.Activa = false;
                            sesion.CerradaPorUsuario = true;
                        }

                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    // Cerrar solo la sesión actual
                    var sesion = await _context.Sesions
                        .FirstOrDefaultAsync(s => s.Token == token && s.Activa == true);

                    if (sesion != null)
                    {
                        sesion.Activa = false;
                        sesion.CerradaPorUsuario = true;
                        await _context.SaveChangesAsync();
                    }
                }

                return Ok(new { message = "Sesión cerrada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar sesión");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("debug-token")]
        public async Task<IActionResult> DebugToken()
        {
            try
            {
                // Obtener el token del encabezado de autorización
                var authHeader = Request.Headers["Authorization"].ToString();

                // Variables para almacenar los resultados
                object session = null;
                object user = null;
                bool isValid = false;
                string token = "";
                string errorMessage = "";

                if (string.IsNullOrEmpty(authHeader))
                {
                    errorMessage = "Encabezado de autorización no proporcionado";
                }
                else
                {
                    // Verificar si el encabezado comienza con "Bearer "
                    if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        // Extraer el token después de "Bearer "
                        token = authHeader.Substring("Bearer ".Length).Trim();
                    }
                    else
                    {
                        // Si no comienza con "Bearer ", asumimos que el encabezado es el token directamente
                        token = authHeader.Trim();
                    }

                    if (string.IsNullOrEmpty(token))
                    {
                        errorMessage = "Token vacío después de extraer del encabezado";
                    }
                    else
                    {
                        // Buscar la sesión activa con ese token
                        var sesion = await _context.Sesions
                            .Include(s => s.IdUsuarioNavigation)
                            .FirstOrDefaultAsync(s => s.Token == token && s.Activa == true && s.FechaExpiracion > DateTime.Now);

                        if (sesion == null)
                        {
                            errorMessage = "No se encontró una sesión activa con ese token";

                            // Verificar si el token existe pero está inactivo o expirado
                            var sesionInactiva = await _context.Sesions
                                .FirstOrDefaultAsync(s => s.Token == token);

                            if (sesionInactiva != null)
                            {
                                if (sesionInactiva.Activa != true)
                                {
                                    errorMessage += " (la sesión existe pero no está activa)";
                                }
                                else if (sesionInactiva.FechaExpiracion <= DateTime.Now)
                                {
                                    errorMessage += $" (la sesión ha expirado el {sesionInactiva.FechaExpiracion})";
                                }
                            }
                        }
                        else
                        {
                            isValid = true;
                            session = new
                            {
                                IdSesion = sesion.IdSesion,
                                IdUsuario = sesion.IdUsuario,
                                Token = sesion.Token,
                                FechaInicio = sesion.FechaInicio,
                                FechaExpiracion = sesion.FechaExpiracion,
                                Activa = sesion.Activa
                            };

                            user = new
                            {
                                IdUsuario = sesion.IdUsuarioNavigation.IdUsuario,
                                CorreoElectronico = sesion.IdUsuarioNavigation.CorreoElectronico,
                                Nombre = sesion.IdUsuarioNavigation.Nombre,
                                Apellidos = sesion.IdUsuarioNavigation.Apellidos
                            };
                        }
                    }
                }

                // Crear el objeto result con todos los valores
                var result = new
                {
                    AuthHeader = authHeader,
                    Token = token,
                    IsValid = isValid,
                    Session = session,
                    User = user,
                    ErrorMessage = errorMessage
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("server-time")]
        public IActionResult GetServerTime()
        {
            return Ok(new
            {
                ServerTime = DateTime.Now,
                FormattedTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
            });
        }
        public class LogoutRequest
        {
            public bool CerrarEnTodos { get; set; }
        }
    }

}