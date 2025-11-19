using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WebApi.Models;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly dbJoinnusContext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly IWebHostEnvironment _env;

        public AuthController(dbJoinnusContext context, ILogger<AuthController> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                //AGREGADO ***********************************************************************
                _logger.LogInformation($"Intento de login para: {request.CorreoElectronico}");
                //*********************************************************************************

                // Validar que se proporcionaron correo y contraseña
                if (string.IsNullOrEmpty(request.CorreoElectronico) || string.IsNullOrEmpty(request.Contrasena))
                {
                    return BadRequest(new { message = "Correo electrónico y contraseña son requeridos" });
                }

                // Buscar al usuario por correo electrónico
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.CorreoElectronico == request.CorreoElectronico);

                // Si el usuario no existe
                if (usuario == null)
                {
                    //*********************************************************************************
                    _logger.LogWarning($"Usuario no encontrado: {request.CorreoElectronico}");
                    //*********************************************************************************
                    return NotFound(new { message = "Usuario no encontrado" });
                }
                //******************  AGREGADO    ***************************************
                _logger.LogInformation($"Usuario encontrado: {usuario.IdUsuario}");
                //*********************************************************

                // Verificar si la cuenta está bloqueada
                if (usuario.Bloqueado == true)
                {
                    // Verificar si ha pasado el tiempo de bloqueo (15 minutos)
                    if (usuario.UltimoIntentoFallido.HasValue &&
                        DateTime.Now < usuario.UltimoIntentoFallido.Value.AddMinutes(15))
                    {
                        _logger.LogWarning($"Cuenta bloqueada: {request.CorreoElectronico}");
                        return BadRequest(new { message = "Cuenta bloqueada temporalmente. Intente más tarde." });
                    }
                    else
                    {
                        // Desbloquear la cuenta
                        usuario.Bloqueado = false;
                        usuario.IntentosFallidos = 0;
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Cuenta desbloqueada: {request.CorreoElectronico}");
                    }
                }

                // Verificar contraseña
                if (!VerifyPassword(request.Contrasena, usuario.ContrasenaHash))
                {
                    // Incrementar intentos fallidos
                    usuario.IntentosFallidos = (usuario.IntentosFallidos ?? 0) + 1;
                    usuario.UltimoIntentoFallido = DateTime.Now;

                    // Si llega a 3 intentos, bloquear
                    if (usuario.IntentosFallidos >= 3)
                    {
                        usuario.Bloqueado = true;
                        _logger.LogWarning($"Cuenta bloqueada por intentos fallidos: {request.CorreoElectronico}");
                    }

                    await _context.SaveChangesAsync();

                    return BadRequest(new { message = "Contraseña incorrecta" });
                }

                // Restablecer intentos fallidos
                usuario.IntentosFallidos = 0;
                usuario.UltimoIntentoFallido = null;
                await _context.SaveChangesAsync();

                // Crear sesión
                var sesion = new Sesion
                {
                    IdUsuario = usuario.IdUsuario,
                    Token = GenerateToken(),
                    FechaInicio = DateTime.Now,
                    FechaExpiracion = DateTime.Now.AddHours(2), // Sesión de 2 horas
                    NroIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Dispositivo = Request.Headers["User-Agent"].ToString(),
                    Activa = true,
                    CerradaPorUsuario = false

                };

                _context.Sesions.Add(sesion);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Login exitoso para: {request.CorreoElectronico}");

                // Respuesta exitosa
                return Ok(new
                {
                    message = "Acceso concedido",
                    token = sesion.Token,
                    usuario = new
                    {
                        id = usuario.IdUsuario,
                        nombre = usuario.Nombre,
                        apellidos = usuario.Apellidos,
                        correo = usuario.CorreoElectronico
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el proceso de login");
                // En desarrollo, devolver el detalle de la excepción
                if (_env.IsDevelopment())
                {
                    return StatusCode(500, new
                    {
                        message = ex.Message,
                        stackTrace = ex.StackTrace,
                        innerException = ex.InnerException?.Message
                    });
                }
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            try
            {
                // Implementar la verificación de contraseña (usando el mismo método que se usó para hashear)
                // Aquí un ejemplo simple - en producción usarías un método más seguro como BCrypt
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

        private string GenerateToken()
        {
            // Generar un token aleatorio para la sesión
            using (var rng = RandomNumberGenerator.Create())
            {
                var tokenBytes = new byte[32];
                rng.GetBytes(tokenBytes);
                return Convert.ToBase64String(tokenBytes).Replace("+", "").Replace("/", "").Replace("=", "");
            }
        }

        public class LoginRequest
        {
            public string CorreoElectronico { get; set; }
            public string Contrasena { get; set; }
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Validar que todos los campos obligatorios estén presentes
                if (string.IsNullOrEmpty(request.CorreoElectronico) ||
                    string.IsNullOrEmpty(request.Contrasena) ||
                    string.IsNullOrEmpty(request.Nombre) ||
                    string.IsNullOrEmpty(request.Apellidos) ||
                    string.IsNullOrEmpty(request.Pais) ||
                    string.IsNullOrEmpty(request.Ciudad) ||
                    string.IsNullOrEmpty(request.TipoDocumento) ||
                    string.IsNullOrEmpty(request.NumeroDocumento))
                {
                    return BadRequest(new { message = "Todos los campos son obligatorios" });
                }

                // Validar formato de correo electrónico
                if (!IsValidEmail(request.CorreoElectronico))
                {
                    return BadRequest(new { message = "Formato de correo electrónico inválido" });
                }

                // Validar que el correo no esté registrado
                if (await _context.Usuarios.AnyAsync(u => u.CorreoElectronico == request.CorreoElectronico))
                {
                    return BadRequest(new { message = "El correo electrónico ya está registrado" });
                }

                // Validar contraseña
                if (!IsValidPassword(request.Contrasena))
                {
                    return BadRequest(new { message = "La contraseña debe tener al menos 8 caracteres, incluyendo mayúsculas, minúsculas, números y caracteres especiales" });
                }

                // Crear el usuario
                var usuario = new Usuario
                {
                    CorreoElectronico = request.CorreoElectronico,
                    ContrasenaHash = HashPassword(request.Contrasena),
                    Nombre = request.Nombre,
                    Apellidos = request.Apellidos,
                    Pais = request.Pais,
                    Ciudad = request.Ciudad,
                    TipoDocumento = request.TipoDocumento,
                    NumeroDocumento = request.NumeroDocumento,
                    Genero = request.Genero,
                    FechaRegistro = DateTime.Now,
                    VerificadoEmail = false, // Se verificará después
                    Bloqueado = false,
                    IntentosFallidos = 0,
                    Activo = true
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // Asignar rol de usuario por defecto
                var tipoUsuario = await _context.TipoUsuarios.FirstOrDefaultAsync(t => t.Nombre == "Usuario");
                if (tipoUsuario != null)
                {
                    var usuarioRol = new UsuarioRol
                    {
                        IdUsuario = usuario.IdUsuario,
                        IdTipoUsuario = tipoUsuario.IdTipoUsuario,
                        FechaAsignacion = DateTime.Now,
                        Activo = true
                    };
                    _context.UsuarioRols.Add(usuarioRol);
                    await _context.SaveChangesAsync();
                }

                // Enviar email de confirmación (simulado)
                // En una implementación real, aquí enviarías un correo electrónico con un enlace de verificación
                _logger.LogInformation($"Email de confirmación enviado a {request.CorreoElectronico}");

                return Ok(new
                {
                    message = "Cuenta creada exitosamente. Por favor verifica tu correo electrónico.",
                    usuario = new
                    {
                        id = usuario.IdUsuario,
                        nombre = usuario.Nombre,
                        apellidos = usuario.Apellidos,
                        correo = usuario.CorreoElectronico
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el proceso de registro");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPassword(string password)
        {
            // Mínimo 8 caracteres, al menos una mayúscula, una minúscula, un número y un carácter especial
            var regex = new System.Text.RegularExpressions.Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
            return regex.IsMatch(password);
        }

        private string HashPassword(string password)
        {
            // Implementar el hashing de contraseña (usando SHA256 como ejemplo simple)
            // En producción, usarías un método más seguro como BCrypt o ASP.NET Core Identity
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public class RegisterRequest
        {
            public string CorreoElectronico { get; set; }
            public string Contrasena { get; set; }
            public string Nombre { get; set; }
            public string Apellidos { get; set; }
            public string Pais { get; set; }
            public string Ciudad { get; set; }
            public string TipoDocumento { get; set; }
            public string NumeroDocumento { get; set; }
            public string Genero { get; set; }
            public bool AceptaTerminos { get; set; }
            public bool AceptaPromociones { get; set; }
        }

        [HttpPost("recuperar-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                // Validar que se proporcionó el correo
                if (string.IsNullOrEmpty(request.CorreoElectronico))
                {
                    return BadRequest(new { message = "El correo electrónico es requerido" });
                }

                // Buscar al usuario por correo electrónico
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.CorreoElectronico == request.CorreoElectronico);

                // Si el usuario no existe, no revelamos esta información por seguridad
                if (usuario == null)
                {
                    return Ok(new { message = "Si el correo electrónico está registrado, recibirás un enlace para restablecer tu contraseña" });
                }

                // Generar token de restablecimiento
                var token = GenerateToken();
                var expiryTime = DateTime.Now.AddHours(1); // Token válido por 1 hora

                // Guardar el token en la base de datos
                usuario.RestablecerToken = token;
                usuario.RestablecerExpira = expiryTime;
                await _context.SaveChangesAsync();

                // Enviar email con el enlace de restablecimiento (simulado)
                var resetLink = $"https://myaccount.joinnus.com/auth/reset/{token}";
                _logger.LogInformation($"Enlace de restablecimiento enviado a {request.CorreoElectronico}: {resetLink}");

                return Ok(new { message = "Si el correo electrónico está registrado, recibirás un enlace para restablecer tu contraseña" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el proceso de recuperación de contraseña");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        public class ForgotPasswordRequest
        {
            public string CorreoElectronico { get; set; }
        }

        [HttpPost("restablecer-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                // Validar que se proporcionaron todos los campos
                if (string.IsNullOrEmpty(request.Token) ||
                    string.IsNullOrEmpty(request.NuevaContrasena) ||
                    string.IsNullOrEmpty(request.ConfirmacionContrasena))
                {
                    return BadRequest(new { message = "Todos los campos son obligatorios" });
                }

                // Validar que las contraseñas coincidan
                if (request.NuevaContrasena != request.ConfirmacionContrasena)
                {
                    return BadRequest(new { message = "Las contraseñas no coinciden" });
                }

                // Validar la nueva contraseña
                if (!IsValidPassword(request.NuevaContrasena))
                {
                    return BadRequest(new { message = "La contraseña debe tener al menos 8 caracteres, incluyendo mayúsculas, minúsculas, números y caracteres especiales" });
                }

                // Buscar al usuario por el token de restablecimiento
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.RestablecerToken == request.Token);

                // Si no se encuentra el usuario o el token ha expirado
                if (usuario == null || usuario.RestablecerExpira < DateTime.Now)
                {
                    return BadRequest(new { message = "Token inválido o expirado" });
                }

                // Verificar que la nueva contraseña sea diferente a la anterior
                if (VerifyPassword(request.NuevaContrasena, usuario.ContrasenaHash))
                {
                    return BadRequest(new { message = "La nueva contraseña debe ser diferente a la anterior" });
                }

                // Actualizar la contraseña
                usuario.ContrasenaHash = HashPassword(request.NuevaContrasena);
                usuario.RestablecerToken = null;
                usuario.RestablecerExpira = null;
                await _context.SaveChangesAsync();

                // Enviar notificación de cambio de contraseña (simulado)
                _logger.LogInformation($"Contraseña actualizada para el usuario {usuario.CorreoElectronico}");

                return Ok(new { message = "Contraseña actualizada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el proceso de restablecimiento de contraseña");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        public class ResetPasswordRequest
        {
            public string Token { get; set; }
            public string NuevaContrasena { get; set; }
            public string ConfirmacionContrasena { get; set; }
        }
    }


}