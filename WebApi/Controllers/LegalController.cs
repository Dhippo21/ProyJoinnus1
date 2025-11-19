using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/Legal")]
    [ApiController]
    public class LegalController : ControllerBase
    {
        private readonly dbJoinnusContext _context;
        private readonly ILogger<LegalController> _logger;
        private readonly IWebHostEnvironment _env;

        public LegalController(dbJoinnusContext context, ILogger<LegalController> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
        }

        // Servicio: "Preguntas frecuentes"
        [HttpGet("faq")]
        public async Task<IActionResult> GetFaqs()
        {
            var faqs = await _context.Faqs
                .Where(f => f.Activo == true)
                .Select(f => new FaqDto
                {
                    IdPregunta = f.IdPregunta,
                    Categoria = f.Categoria,
                    Pregunta = f.Pregunta,
                    Respuesta = f.Respuesta
                })
                .ToListAsync();

            return Ok(faqs);
        }

        // Servicio: "Términos y condiciones"
        [HttpGet("terminos-y-condiciones")]
        public async Task<IActionResult> GetTerminos()
        {
            var terminos = await _context.PoliticaLegal
                .Where(p => p.Tipo == "Terminos y Condiciones")
                .OrderByDescending(p => p.Version)
                .FirstOrDefaultAsync();

            if (terminos == null) return NotFound();

            return Ok(new PoliticaLegalDto
            {
                IdPolitica = terminos.IdPolitica,
                Tipo = terminos.Tipo,
                Contenido = terminos.Contenido,
                Version = terminos.Version
            });
        }

        // Servicio: "Política de privacidad"
        [HttpGet("politica-de-privacidad")]
        public async Task<IActionResult> GetPrivacidad()
        {
            var politica = await _context.PoliticaLegal
                .Where(p => p.Tipo == "Privacidad")
                .OrderByDescending(p => p.Version)
                .FirstOrDefaultAsync();

            if (politica == null) return NotFound();

            return Ok(new PoliticaLegalDto
            {
                IdPolitica = politica.IdPolitica,
                Tipo = politica.Tipo,
                Contenido = politica.Contenido,
                Version = politica.Version
            });
        }

        // Servicio: "Libro de reclamaciones"
        [HttpPost("reclamo")]
        public async Task<IActionResult> CrearReclamo([FromBody] ReclamoDto dto)
        {
            var reclamo = new LibroReclamacione
            {
                TipoDocumento = dto.TipoDocumento,
                NumeroDocumento = dto.NumeroDocumento,
                NombreCompleto = dto.NombreCompleto,
                CorreoContacto = dto.CorreoContacto,
                TelefonoContacto = dto.TelefonoContacto,
                TipoReclamo = dto.TipoReclamo,
                Detalle = dto.Detalle,
                Estado = "Recibido",
                NumeroRegistro = "JR-" + DateTime.Now.Year + "-" + new Random().Next(10000, 99999)
            };

            _context.LibroReclamaciones.Add(reclamo);
            await _context.SaveChangesAsync();

            return Ok(new { NumeroRegistro = reclamo.NumeroRegistro, Mensaje = "Reclamo registrado con éxito." });
        }
    } // <--- FIN DEL CONTROLLER

    // --- DTOs (FUERA DE LA CLASE) ---

    public class FaqDto
    {
        public int IdPregunta { get; set; }
        public string Categoria { get; set; }
        public string Pregunta { get; set; }
        public string Respuesta { get; set; }
    }

    public class ReclamoDto
    {
        [Required]
        public string TipoDocumento { get; set; }
        [Required]
        public string NumeroDocumento { get; set; }
        [Required]
        public string NombreCompleto { get; set; }
        [Required]
        [EmailAddress]
        public string CorreoContacto { get; set; }
        [Required]
        public string TelefonoContacto { get; set; }
        [Required]
        public string TipoReclamo { get; set; }
        [Required]
        public string Detalle { get; set; }
    }

    public class PoliticaLegalDto
    {
        public int IdPolitica { get; set; }
        public string Tipo { get; set; }
        public string Contenido { get; set; }
        public string Version { get; set; }
    }
}