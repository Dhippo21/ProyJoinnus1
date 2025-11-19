using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WebApi.Models;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventoController : ControllerBase
    {
        private readonly dbJoinnusContext _context;
        private readonly ILogger<EventoController> _logger;

        public EventoController(dbJoinnusContext context, ILogger<EventoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Servicio: Destacados
        [HttpGet("destacados")]
        public async Task<IActionResult> GetFeaturedEvents()
        {
            var eventos = await _context.Eventos
                .Where(e => e.Activo == true && e.Cancelado == false && (e.Destacado == true || e.EnTendencia == true))
                .OrderByDescending(e => e.FechaCreacion)
                .Take(8)
                .Select(e => new EventoResumen
                {
                    IdEvento = e.IdEvento,
                    Titulo = e.Titulo,
                    ImagenPortada = e.ImagenPortada,
                    FechaInicio = e.FechaInicio,
                    NombreLugar = e.IdLugarNavigation.Nombre,
                    NombreCiudad = e.IdLugarNavigation.IdCiudadNavigation.Nombre,
                    PrecioMinimo = e.TipoEntrada.Min(t => t.Precio) // Calculo simple
                })
                .ToListAsync();

            return Ok(eventos);
        }

        // Servicio: Buscar con filtros
        [HttpGet("buscar")]
        public async Task<IActionResult> SearchEvents([FromQuery] FiltroEventos filtro)
        {
            var query = _context.Eventos
                .Include(e => e.IdLugarNavigation.IdCiudadNavigation)
                .Where(e => e.Activo == true && e.Cancelado == false);

            if (!string.IsNullOrEmpty(filtro.Termino))
            {
                query = query.Where(e => e.Titulo.Contains(filtro.Termino) || e.Descripcion.Contains(filtro.Termino));
            }
            if (filtro.IdCategoria.HasValue)
            {
                query = query.Where(e => e.IdCategoria == filtro.IdCategoria.Value);
            }
            if (filtro.IdCiudad.HasValue)
            {
                query = query.Where(e => e.IdLugarNavigation.IdCiudad == filtro.IdCiudad.Value);
            }
            if (filtro.Fecha.HasValue)
            {
                query = query.Where(e => e.FechaInicio.Date == filtro.Fecha.Value.Date);
            }

            var eventos = await query
                .OrderBy(e => e.FechaInicio)
                .Select(e => new EventoResumen
                {
                    IdEvento = e.IdEvento,
                    Titulo = e.Titulo,
                    ImagenPortada = e.ImagenPortada,
                    FechaInicio = e.FechaInicio,
                    NombreLugar = e.IdLugarNavigation.Nombre,
                    NombreCiudad = e.IdLugarNavigation.IdCiudadNavigation.Nombre,
                    PrecioMinimo = e.TipoEntrada.Any() ? e.TipoEntrada.Min(t => t.Precio) : 0
                })
                .ToListAsync();

            return Ok(eventos);
        }

        // Servicio: Tendencias
        [HttpGet("tendencias")]
        public async Task<IActionResult> GetTrendingEvents()
        {
            var fechaLimite = DateTime.Now.AddDays(-30);
            var eventos = await _context.Eventos
                .Where(e => e.Activo == true && e.Cancelado == false && e.EnTendencia == true && e.FechaCreacion >= fechaLimite)
                .Take(12)
                .Select(e => new EventoResumen
                {
                    IdEvento = e.IdEvento,
                    Titulo = e.Titulo,
                    ImagenPortada = e.ImagenPortada,
                    FechaInicio = e.FechaInicio,
                    NombreLugar = e.IdLugarNavigation.Nombre,
                    NombreCiudad = e.IdLugarNavigation.IdCiudadNavigation.Nombre,
                    PrecioMinimo = e.TipoEntrada.Any() ? e.TipoEntrada.Min(t => t.Precio) : 0
                })
                .ToListAsync();

            return Ok(eventos);
        }

        // Servicio: Detalle de Evento
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetalleEvento(int id)
        {
            var evento = await _context.Eventos
                .Include(e => e.IdLugarNavigation.IdCiudadNavigation)
                .Include(e => e.IdCategoriaNavigation)
                .Include(e => e.IdOrganizadorNavigation)
                .Include(e => e.TipoEntrada)
                .Where(e => e.IdEvento == id && e.Activo == true)
                .Select(e => new EventoDetalle
                {
                    IdEvento = e.IdEvento,
                    Titulo = e.Titulo,
                    Descripcion = e.Descripcion,
                    FechaInicio = e.FechaInicio,
                    ImagenBanner = e.ImagenBanner,
                    Lugar = e.IdLugarNavigation.Nombre,
                    Ciudad = e.IdLugarNavigation.IdCiudadNavigation.Nombre,
                    Categoria = e.IdCategoriaNavigation.Nombre,
                    Organizador = e.IdOrganizadorNavigation.Nombre,
                    TiposDeEntrada = e.TipoEntrada.Where(t => t.Activo == true).Select(te => new TipoEntradaPublica
                    {
                        IdTipoEntrada = te.IdTipoEntrada,
                        Nombre = te.Nombre,
                        Precio = te.Precio,
                        CupoDisponible = te.CupoDisponible
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (evento == null) return NotFound("Evento no encontrado.");

            return Ok(evento);
        }

        // Servicio: Listar Categorias
        [HttpGet("categorias")]
        public async Task<IActionResult> GetCategorias()
        {
            var categorias = await _context.Categoria
                .Where(c => c.Activo == true)
                .Select(c => new { c.IdCategoria, c.Nombre, c.ImagenUrl })
                .ToListAsync();
            return Ok(categorias);
        }

        // Servicio: Listar Ciudades
        [HttpGet("ciudades")]
        public async Task<IActionResult> GetCiudades()
        {
            var ciudades = await _context.Ciudad
                 .Where(c => c.Activo == true)
                 .Select(c => new { c.IdCiudad, c.Nombre })
                 .ToListAsync();
            return Ok(ciudades);
        }

    } // <--- FIN DEL CONTROLLER EVENTOS

    // --- DTOs EXCLUSIVOS DE EVENTO CONTROLLER ---
    // NOTA: He eliminado los DTOs de Organizador (Crear, Editar, Dashboard) porque NO van aquí.

    public class EventoResumen
    {
        public int IdEvento { get; set; }
        public string Titulo { get; set; }
        public string ImagenPortada { get; set; }
        public DateTime FechaInicio { get; set; }
        public string NombreLugar { get; set; }
        public string NombreCiudad { get; set; }
        public decimal PrecioMinimo { get; set; }
    }

    public class EventoDetalle
    {
        public int IdEvento { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public string ImagenBanner { get; set; }
        public string Lugar { get; set; }
        public string Ciudad { get; set; }
        public string Categoria { get; set; }
        public string Organizador { get; set; }
        public List<TipoEntradaPublica> TiposDeEntrada { get; set; }
    }

    public class TipoEntradaPublica
    {
        public int IdTipoEntrada { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int CupoDisponible { get; set; }
    }

    public class FiltroEventos
    {
        public string? Termino { get; set; }
        public DateTime? Fecha { get; set; }
        public int? IdCategoria { get; set; }
        public int? IdCiudad { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
    }
}