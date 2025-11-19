using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/eventos")]
    public class EventoController : ControllerBase
    {
        private readonly dbJoinnusContext _context;
        private readonly ILogger<EventoController> _logger;

        public EventoController(dbJoinnusContext context, ILogger<EventoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("destacados")] //**********************************************************************************
        public async Task<IActionResult> GetFeaturedEvents()
        {
            try
            {
                _logger.LogInformation("Iniciando GetFeaturedEvents");

                // Obtener eventos destacados o populares
                var eventosDestacados = await _context.Eventos
                    .Where(e => e.Activo == true && e.Cancelado == false &&
                               (e.Destacado == true || e.EnTendencia == true))
                    .OrderByDescending(e => e.FechaCreacion)
                    .Take(8)
                    .Select(e => new
                    {
                        IdEvento = e.IdEvento,
                        Titulo = e.Titulo,
                        Descripcion = e.Descripcion,
                        FechaInicio = e.FechaInicio,
                        FechaFin = e.FechaFin,
                        HoraEvento = e.HoraEvento,
                        ImagenPortada = e.ImagenPortada,
                        ImagenBanner = e.ImagenBanner,
                        UrlAmigable = e.UrlAmigable,
                        Destacado = e.Destacado,
                        EnTendencia = e.EnTendencia,
                        IdLugar = e.IdLugar,
                        IdCategoria = e.IdCategoria
                    })
                    .ToListAsync();

                _logger.LogInformation($"Se encontraron {eventosDestacados.Count} eventos destacados");
                return Ok(eventosDestacados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener eventos destacados");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        //Servicio 14 - Buscar Eventos por Palabras o Filtros
        [HttpGet("buscar")] //************************************************************************************************
        public async Task<IActionResult> SearchEvents(
                [FromQuery] string? termino = null,
                [FromQuery] decimal? precioMinimo = null,
                [FromQuery] decimal? precioMaximo = null,
                [FromQuery] int? idCategoria = null,
                [FromQuery] int? idCiudad = null,
                [FromQuery] DateTime? fechaInicio = null,
                [FromQuery] DateTime? fechaFin = null,
                [FromQuery] int? idLugar = null)
        {
            try
            {
                _logger.LogInformation("Iniciando SearchEvents con parámetros: " +
                    $"término={termino}, precioMinimo={precioMinimo}, precioMaximo={precioMaximo}, " +
                    $"idCategoria={idCategoria}, idCiudad={idCiudad}, fechaInicio={fechaInicio}, " +
                    $"fechaFin={fechaFin}, idLugar={idLugar}");

                // Iniciar la consulta base
                var query = _context.Eventos
                    .Include(e => e.IdLugarNavigation)
                    .Include(e => e.IdCategoriaNavigation)
                    .Where(e => e.Activo == true && e.Cancelado == false);

                // Aplicar filtros según los parámetros proporcionados
                if (!string.IsNullOrEmpty(termino))
                {
                    query = query.Where(e =>
                        e.Titulo.Contains(termino) ||
                        e.Descripcion.Contains(termino));
                }

                if (precioMinimo.HasValue)
                {
                    query = query.Where(e =>
                        e.TipoEntrada.Any(te => te.Precio >= precioMinimo.Value));
                }

                if (precioMaximo.HasValue)
                {
                    query = query.Where(e =>
                        e.TipoEntrada.Any(te => te.Precio <= precioMaximo.Value));
                }

                if (idCategoria.HasValue)
                {
                    query = query.Where(e => e.IdCategoria == idCategoria.Value);
                }

                if (idCiudad.HasValue)
                {
                    query = query.Where(e => e.IdLugarNavigation.IdCiudad == idCiudad.Value);
                }

                if (fechaInicio.HasValue)
                {
                    query = query.Where(e => e.FechaInicio >= fechaInicio.Value);
                }

                if (fechaFin.HasValue)
                {
                    query = query.Where(e => e.FechaInicio <= fechaFin.Value);
                }

                if (idLugar.HasValue)
                {
                    query = query.Where(e => e.IdLugar == idLugar.Value);
                }

                // Ejecutar la consulta y ordenar por fecha ascendente
                var eventos = await query
                    .OrderBy(e => e.FechaInicio)
                    .Select(e => new
                    {
                        IdEvento = e.IdEvento,
                        Titulo = e.Titulo,
                        Descripcion = e.Descripcion,
                        FechaInicio = e.FechaInicio,
                        FechaFin = e.FechaFin,
                        HoraEvento = e.HoraEvento,
                        ImagenPortada = e.ImagenPortada,
                        UrlAmigable = e.UrlAmigable,
                        Ubicacion = new
                        {
                            IdLugar = e.IdLugar,
                            NombreLugar = e.IdLugarNavigation.Nombre,
                            Direccion = e.IdLugarNavigation.Direccion,
                            Ciudad = e.IdLugarNavigation.IdCiudadNavigation.Nombre
                        },
                        Categoria = new
                        {
                            IdCategoria = e.IdCategoria,
                            NombreCategoria = e.IdCategoriaNavigation.Nombre
                        },
                        Precios = e.TipoEntrada.Select(te => new
                        {
                            IdTipoEntrada = te.IdTipoEntrada,
                            Nombre = te.Nombre,
                            Precio = te.Precio
                        }).ToList()
                    })
                    .ToListAsync();

                _logger.LogInformation($"Se encontraron {eventos.Count} eventos que coinciden con los criterios de búsqueda");
                return Ok(eventos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar eventos");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        //Servicio 15 - Ver Eventos en Tendencia
        [HttpGet("tendencias")] //*******************************************************************************************************************
        public async Task<IActionResult> GetTrendingEvents()
        {
            try
            {
                _logger.LogInformation("Iniciando GetTrendingEvents");

                // Calcular la fecha de hace 30 días
                var fechaHace7Dias = DateTime.Now.AddDays(-30);

                // Obtener eventos en tendencia
                var eventosTendencia = await _context.Eventos
                    .Include(e => e.IdLugarNavigation)
                    .Include(e => e.IdCategoriaNavigation)
                    .Where(e =>
                        e.Activo == true &&
                        e.Cancelado == false &&
                        e.EnTendencia == true &&
                        e.FechaCreacion >= fechaHace7Dias)
                    .OrderByDescending(e => e.FechaActualizacion)
                    .Take(12)
                    .Select(e => new
                    {
                        IdEvento = e.IdEvento,
                        Titulo = e.Titulo,
                        Descripcion = e.Descripcion,
                        FechaInicio = e.FechaInicio,
                        FechaFin = e.FechaFin,
                        HoraEvento = e.HoraEvento,
                        ImagenPortada = e.ImagenPortada,
                        UrlAmigable = e.UrlAmigable,
                        Ubicacion = new
                        {
                            IdLugar = e.IdLugar,
                            NombreLugar = e.IdLugarNavigation.Nombre,
                            Ciudad = e.IdLugarNavigation.IdCiudadNavigation.Nombre
                        },
                        Categoria = new
                        {
                            IdCategoria = e.IdCategoria,
                            NombreCategoria = e.IdCategoriaNavigation.Nombre
                        }
                    })
                    .ToListAsync();

                _logger.LogInformation($"Se encontraron {eventosTendencia.Count} eventos en tendencia");
                return Ok(eventosTendencia);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener eventos en tendencia");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
        //Servicio 16 - Eventos que Podrían Gustarte: eventos según lo que buscaste o compraste ==============================================================
        [HttpGet("recomendados")]
        public async Task<IActionResult> RecommendEvents()
        {
            try
            {
                _logger.LogInformation("Iniciando RecommendEvents");

                // Verificar si hay un usuario autenticado
                if (!HttpContext.Items.ContainsKey("User"))
                {
                    _logger.LogWarning("Usuario no autenticado");
                    return Unauthorized(new { message = "Se requiere autenticación para ver recomendaciones personalizadas" });
                }

                var usuario = (Usuario)HttpContext.Items["User"];
                _logger.LogInformation($"Usuario autenticado: {usuario.CorreoElectronico} (ID: {usuario.IdUsuario})");

                List<int> categoriasInteres = new List<int>();
                List<int> ciudadesInteres = new List<int>();
                bool tieneHistorial = false;

                // Obtener el historial de búsquedas del usuario
                var historialBusquedas = await _context.HistorialBusqueda
                    .Where(h => h.IdUsuario == usuario.IdUsuario)
                    .OrderByDescending(h => h.FechaBusqueda)
                    .Take(5)
                    .ToListAsync();

                _logger.LogInformation($"Historial de búsquedas encontrado: {historialBusquedas.Count} registros");

                // Obtener los IDs de eventos comprados por el usuario (sin incluir columnas de tipo texto)
                var idsEventosComprados = await _context.Compras
                    .Where(c => c.IdUsuario == usuario.IdUsuario)
                    .Select(c => c.IdEvento)
                    .Distinct()
                    .ToListAsync();

                _logger.LogInformation($"Eventos comprados: {idsEventosComprados.Count} registros");

                // Obtener los detalles de los eventos comprados por el usuario (solo si hay compras)
                List<Evento> comprasUsuario = new List<Evento>();
                if (idsEventosComprados.Any())
                {
                    comprasUsuario = await _context.Eventos
                        .Where(e => idsEventosComprados.Contains(e.IdEvento))
                        .ToListAsync();
                }

                _logger.LogInformation($"Detalles de compras de usuario: {comprasUsuario.Count} registros");

                // Verificar si el usuario tiene historial
                tieneHistorial = historialBusquedas.Any() || comprasUsuario.Any();
                _logger.LogInformation($"¿Tiene historial?: {tieneHistorial}");

                if (tieneHistorial)
                {
                    // Extraer categorías y ciudades del historial de búsquedas
                    if (historialBusquedas.Any())
                    {
                        // Para categorías (IdCategoria es nullable en HistorialBusqueda)
                        var categoriasHistorial = historialBusquedas
                            .Where(h => h.IdCategoria.HasValue)
                            .Select(h => h.IdCategoria.Value)
                            .ToList() // Ejecutar la consulta en la base de datos
                            .Distinct(); // Aplicar DISTINCT en memoria

                        categoriasInteres.AddRange(categoriasHistorial);
                        _logger.LogInformation($"Categorías del historial: {categoriasInteres.Count}");

                        // Para ciudades (IdCiudad es nullable en HistorialBusqueda)
                        var ciudadesHistorial = historialBusquedas
                            .Where(h => h.IdCiudad.HasValue)
                            .Select(h => h.IdCiudad.Value)
                            .ToList() // Ejecutar la consulta en la base de datos
                            .Distinct(); // Aplicar DISTINCT en memoria

                        ciudadesInteres.AddRange(ciudadesHistorial);
                        _logger.LogInformation($"Ciudades del historial: {ciudadesInteres.Count}");
                    }

                    // Extraer categorías y ciudades de las compras
                    if (comprasUsuario.Any())
                    {
                        // Para categorías (IdCategoria no es nullable en Evento)
                        var categoriasCompras = comprasUsuario
                            .Select(e => e.IdCategoria)
                            .ToList() // Ejecutar la consulta en la base de datos
                            .Distinct(); // Aplicar DISTINCT en memoria

                        categoriasInteres.AddRange(categoriasCompras);
                        _logger.LogInformation($"Categorías de compras: {categoriasCompras.Count()}");

                        // Para ciudades (IdCiudad no es nullable en Lugar)
                        var ciudadesCompras = comprasUsuario
                            .Where(e => e.IdLugarNavigation != null)
                            .Select(e => e.IdLugarNavigation.IdCiudad)
                            .ToList() // Ejecutar la consulta en la base de datos
                            .Distinct(); // Aplicar DISTINCT en memoria

                        ciudadesInteres.AddRange(ciudadesCompras);
                        _logger.LogInformation($"Ciudades de compras: {ciudadesCompras.Count()}");
                    }

                    // Eliminar duplicados
                    categoriasInteres = categoriasInteres.Distinct().ToList();
                    ciudadesInteres = ciudadesInteres.Distinct().ToList();
                    _logger.LogInformation($"Categorías totales: {categoriasInteres.Count}, Ciudades totales: {ciudadesInteres.Count}");
                }

                List<object> eventosRecomendados = new List<object>();

                if (tieneHistorial && (categoriasInteres.Any() || ciudadesInteres.Any()))
                {
                    _logger.LogInformation("Buscando eventos relacionados con preferencias del usuario");

                    // Si tenemos categorías o ciudades de interés, buscar eventos relacionados
                    var query = _context.Eventos
                        .Include(e => e.IdLugarNavigation)
                        .Include(e => e.IdCategoriaNavigation)
                        .Where(e => e.Activo == true && e.Cancelado == false);

                    if (categoriasInteres.Any())
                    {
                        query = query.Where(e => categoriasInteres.Contains(e.IdCategoria));
                        _logger.LogInformation($"Filtrando por categorías: {string.Join(", ", categoriasInteres)}");
                    }

                    if (ciudadesInteres.Any())
                    {
                        query = query.Where(e => ciudadesInteres.Contains(e.IdLugarNavigation.IdCiudad));
                        _logger.LogInformation($"Filtrando por ciudades: {string.Join(", ", ciudadesInteres)}");
                    }

                    // Excluir eventos que el usuario ya compró
                    if (idsEventosComprados.Any())
                    {
                        query = query.Where(e => !idsEventosComprados.Contains(e.IdEvento));
                        _logger.LogInformation($"Excluyendo {idsEventosComprados.Count} eventos ya comprados");
                    }

                    var eventosRelacionados = await query
                        .OrderByDescending(e => e.FechaCreacion)
                        .Take(10)
                        .Select(e => new
                        {
                            IdEvento = e.IdEvento,
                            Titulo = e.Titulo,
                            Descripcion = e.Descripcion,
                            FechaInicio = e.FechaInicio,
                            FechaFin = e.FechaFin,
                            HoraEvento = e.HoraEvento,
                            ImagenPortada = e.ImagenPortada,
                            UrlAmigable = e.UrlAmigable,
                            Ubicacion = new
                            {
                                IdLugar = e.IdLugar,
                                NombreLugar = e.IdLugarNavigation.Nombre,
                                Ciudad = e.IdLugarNavigation.IdCiudadNavigation.Nombre
                            },
                            Categoria = new
                            {
                                IdCategoria = e.IdCategoria,
                                NombreCategoria = e.IdCategoriaNavigation.Nombre
                            }
                        })
                        .ToListAsync();

                    eventosRecomendados = eventosRelacionados.Cast<object>().ToList();
                    _logger.LogInformation($"Se encontraron {eventosRecomendados.Count} eventos relacionados");
                }
                else
                {
                    _logger.LogInformation("No tiene historial, mostrando eventos populares genéricos");

                    // Si no tiene historial, mostrar eventos populares genéricos
                    var eventosPopulares = await _context.Eventos
                        .Include(e => e.IdLugarNavigation)
                        .Include(e => e.IdCategoriaNavigation)
                        .Where(e => e.Activo == true && e.Cancelado == false && e.Destacado == true)
                        .OrderByDescending(e => e.FechaCreacion)
                        .Take(10)
                        .Select(e => new
                        {
                            IdEvento = e.IdEvento,
                            Titulo = e.Titulo,
                            Descripcion = e.Descripcion,
                            FechaInicio = e.FechaInicio,
                            FechaFin = e.FechaFin,
                            HoraEvento = e.HoraEvento,
                            ImagenPortada = e.ImagenPortada,
                            UrlAmigable = e.UrlAmigable,
                            Ubicacion = new
                            {
                                IdLugar = e.IdLugar,
                                NombreLugar = e.IdLugarNavigation.Nombre,
                                Ciudad = e.IdLugarNavigation.IdCiudadNavigation.Nombre
                            },
                            Categoria = new
                            {
                                IdCategoria = e.IdCategoria,
                                NombreCategoria = e.IdCategoriaNavigation.Nombre
                            }
                        })
                        .ToListAsync();

                    eventosRecomendados = eventosPopulares.Cast<object>().ToList();
                    _logger.LogInformation($"Se encontraron {eventosRecomendados.Count} eventos populares");
                }

                _logger.LogInformation($"Se generaron {eventosRecomendados.Count} eventos recomendados para el usuario {usuario.CorreoElectronico}");
                return Ok(eventosRecomendados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener eventos recomendados");

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

        // Servicio 17 - Ver Todas las Categorías ******************************************************************************
        [HttpGet("categorias")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                _logger.LogInformation("Iniciando GetCategories");

                // Obtener categorías que tienen eventos activos
                var categorias = await _context.Categoria
                    .Where(c => c.Activo == true &&
                               c.Eventos.Any(e => e.Activo == true && e.Cancelado == false))
                    .OrderBy(c => c.Nombre)
                    .Select(c => new
                    {
                        IdCategoria = c.IdCategoria,
                        Nombre = c.Nombre,
                        Descripcion = c.Descripcion,
                        ImagenUrl = c.ImagenUrl,
                        FechaCreacion = c.FechaCreacion
                    })
                    .ToListAsync();

                _logger.LogInformation($"Se encontraron {categorias.Count} categorías activas");
                return Ok(categorias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las categorías");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // Servicio 18 - Ver Ciudades Donde Hay Eventos ******************************************************************************
        [HttpGet("ciudades")]
        public async Task<IActionResult> GetCities()
        {
            try
            {
                _logger.LogInformation("Iniciando GetCities");

                // Obtener ciudades que tienen eventos activos
                var ciudades = await _context.Ciudad
                    .Where(c => c.Activo == true &&
                               c.Lugar.Any(l => l.Eventos.Any(e => e.Activo == true && e.Cancelado == false)))
                    .OrderBy(c => c.Nombre)
                    .Select(c => new
                    {
                        IdCiudad = c.IdCiudad,
                        Nombre = c.Nombre,
                        Pais = c.Pais,
                        FechaCreacion = c.FechaCreacion
                    })
                    .ToListAsync();

                _logger.LogInformation($"Se encontraron {ciudades.Count} ciudades con eventos activos");
                return Ok(ciudades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las ciudades");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // Servicio 19 - Ver Toda la Información de un Evento ******************************************************************
        [HttpGet("info_evento_id")]
        public async Task<IActionResult> GetEventDetails(int id)
        {
            try
            {
                _logger.LogInformation($"Iniciando GetEventDetails para el evento ID: {id}");

                // Obtener el evento con todos sus detalles relacionados
                var evento = await _context.Eventos
                    .Include(e => e.IdLugarNavigation)
                        .ThenInclude(l => l.IdCiudadNavigation)
                    .Include(e => e.IdCategoriaNavigation)
                    .Include(e => e.TipoEntrada)
                    .Where(e => e.IdEvento == id)
                    .FirstOrDefaultAsync();

                if (evento == null)
                {
                    _logger.LogWarning($"Evento con ID {id} no encontrado");
                    return NotFound(new { message = "Evento no encontrado" });
                }

                // Verificar el estado del evento
                string estadoEvento = "Activo";
                if (evento.Cancelado == true)
                {
                    estadoEvento = "Cancelado";
                }
                else if (evento.FechaInicio < DateTime.Now)
                {
                    estadoEvento = "Finalizado";
                }
                else if (evento.Activo != true)
                {
                    estadoEvento = "Inactivo";
                }

                // Obtener los tipos de entrada con disponibilidad
                var tiposEntrada = evento.TipoEntrada
                    .Where(te => te.Activo == true)
                    .Select(te => new
                    {
                        IdTipoEntrada = te.IdTipoEntrada,
                        Nombre = te.Nombre,
                        Precio = te.Precio,
                        CupoTotal = te.CupoTotal,
                        CupoDisponible = te.CupoDisponible,
                        FechaInicioVenta = te.FechaInicioVenta,
                        FechaFinVenta = te.FechaFinVenta,
                        MinimoCompra = te.MinimoCompra,
                        MaximoCompra = te.MaximoCompra
                    })
                    .ToList();

                // Crear el objeto de respuesta
                var eventoDetallado = new
                {
                    IdEvento = evento.IdEvento,
                    Titulo = evento.Titulo,
                    Descripcion = evento.Descripcion,
                    FechaInicio = evento.FechaInicio,
                    FechaFin = evento.FechaFin,
                    HoraEvento = evento.HoraEvento,
                    ImagenPortada = evento.ImagenPortada,
                    ImagenBanner = evento.ImagenBanner,
                    UrlAmigable = evento.UrlAmigable,
                    Estado = estadoEvento,
                    PoliticaReembolso = evento.PoliticaReembolso,
                    FechaCreacion = evento.FechaCreacion,
                    FechaActualizacion = evento.FechaActualizacion,
                    Ubicacion = new
                    {
                        IdLugar = evento.IdLugarNavigation.IdLugar,
                        Nombre = evento.IdLugarNavigation.Nombre,
                        Direccion = evento.IdLugarNavigation.Direccion,
                        Ciudad = evento.IdLugarNavigation.IdCiudadNavigation.Nombre,
                        Pais = evento.IdLugarNavigation.IdCiudadNavigation.Pais,
                        Latitud = evento.IdLugarNavigation.Latitud,
                        Longitud = evento.IdLugarNavigation.Longitud,
                        CapacidadMaxima = evento.IdLugarNavigation.CapacidadMaxima,
                        SitioWeb = evento.IdLugarNavigation.SitioWeb,
                        Telefono = evento.IdLugarNavigation.Telefono
                    },
                    Categoria = new
                    {
                        IdCategoria = evento.IdCategoriaNavigation.IdCategoria,
                        Nombre = evento.IdCategoriaNavigation.Nombre,
                        Descripcion = evento.IdCategoriaNavigation.Descripcion,
                        ImagenUrl = evento.IdCategoriaNavigation.ImagenUrl
                    },
                    TiposEntrada = tiposEntrada,
                    Organizador = new
                    {
                        IdOrganizador = evento.IdOrganizador,
                        // Aquí podrías incluir más detalles del organizador si tuvieras una relación
                    }
                };

                _logger.LogInformation($"Detalles del evento {evento.Titulo} (ID: {id}) obtenidos correctamente");
                return Ok(eventoDetallado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener los detalles del evento ID: {id}");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}