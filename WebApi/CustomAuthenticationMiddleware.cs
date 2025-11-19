using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using WebApi.Models;

namespace WebApi
{
    public class CustomAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomAuthenticationMiddleware> _logger;

        public CustomAuthenticationMiddleware(RequestDelegate next, ILogger<CustomAuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, dbJoinnusContext dbContext)
        {
            // Log para verificar que el middleware se está ejecutando
            _logger.LogInformation($"Middleware ejecutándose para la ruta: {context.Request.Path}");

            // Excluir rutas que no requieren autenticación
            if (context.Request.Path.StartsWithSegments("/api/auth/login") ||
                context.Request.Path.StartsWithSegments("/api/auth/register") ||
                context.Request.Path.StartsWithSegments("/api/auth/forgot-password") ||
                context.Request.Path.StartsWithSegments("/api/auth/reset-password") ||
                context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.StartsWithSegments("/api/usuario/debug-token") ||
                context.Request.Path.StartsWithSegments("/api/usuario/server-time") ||
                context.Request.Path.StartsWithSegments("/api/eventos/destacados") ||
                context.Request.Path.StartsWithSegments("/api/eventos/tendencias") ||
                context.Request.Path.StartsWithSegments("/api/eventos/categorias") ||
                context.Request.Path.StartsWithSegments("/api/eventos/ciudades") ||
                context.Request.Path.StartsWithSegments("/api/eventos/info_evento_id") ||
                context.Request.Path.StartsWithSegments("/api/eventos/buscar"))
            {
                _logger.LogInformation($"Ruta excluida de autenticación: {context.Request.Path}");
                await _next(context);
                return;
            }

            // Obtener el token del encabezado de autorización
            var authHeader = context.Request.Headers["Authorization"].ToString();
            _logger.LogInformation($"Encabezado de autorización recibido: {authHeader}");
            string token = "";

            if (string.IsNullOrEmpty(authHeader))
            {
                _logger.LogWarning("Encabezado de autorización no proporcionado");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "No autorizado: Encabezado de autorización no proporcionado" });
                return;
            }

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

            _logger.LogInformation($"Token extraído: {token}");

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Token vacío después de extraer del encabezado");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "No autorizado: Token vacío" });
                return;
            }

            // Buscar la sesión activa con ese token
            var sesion = await dbContext.Sesions
                .Include(s => s.IdUsuarioNavigation)
                .FirstOrDefaultAsync(s => s.Token == token && s.Activa == true && s.FechaExpiracion > DateTime.Now);

            if (sesion == null)
            {
                _logger.LogWarning($"No se encontró una sesión activa con el token: {token}");

                // Verificar si el token existe pero está inactivo o expirado
                var sesionInactiva = await dbContext.Sesions
                    .FirstOrDefaultAsync(s => s.Token == token);

                if (sesionInactiva != null)
                {
                    if (sesionInactiva.Activa != true)
                    {
                        _logger.LogWarning("La sesión existe pero no está activa");
                    }
                    else if (sesionInactiva.FechaExpiracion <= DateTime.Now)
                    {
                        _logger.LogWarning($"La sesión ha expirado. Fecha de expiración: {sesionInactiva.FechaExpiracion}");
                    }
                }
                else
                {
                    _logger.LogWarning("El token no existe en la base de datos");
                }

                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "No autorizado: Token inválido o expirado" });
                return;
            }

            _logger.LogInformation($"Sesión encontrada para el usuario {sesion.IdUsuarioNavigation.CorreoElectronico}");

            // Crear una identidad para el usuario con el tipo de autenticación
            var claims = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, sesion.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, sesion.IdUsuarioNavigation.Nombre),
                new Claim(ClaimTypes.Email, sesion.IdUsuarioNavigation.CorreoElectronico)
            }, "CustomAuthentication");

            var principal = new ClaimsPrincipal(claims);

            // Establecer el usuario en el contexto de HTTP
            context.User = principal;

            // Agregar el usuario al contexto para que esté disponible en los controladores
            context.Items["User"] = sesion.IdUsuarioNavigation;

            _logger.LogInformation($"Usuario establecido en el contexto: {sesion.IdUsuarioNavigation.CorreoElectronico}");

            await _next(context);
        }
    }
}