using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]")] // Recomendado: usar [controller] para que tome el nombre "Compras"
    [ApiController]
    public class ComprasController : ControllerBase
    {
        private readonly dbJoinnusContext _context;
        private readonly ILogger<ComprasController> _logger;
        private readonly IWebHostEnvironment _env;

        public ComprasController(dbJoinnusContext context, ILogger<ComprasController> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
        }

        [HttpPost("iniciar")]
        public async Task<IActionResult> IniciarCompra([FromBody] IniciarCompraDto dto)
        {
            // Validar que el usuario exista (simulado o por token)
            // var idUsuario = ... 

            var tipoEntrada = await _context.TipoEntrada.FindAsync(dto.IdTipoEntrada);
            if (tipoEntrada == null || tipoEntrada.CupoDisponible < dto.Cantidad)
            {
                return BadRequest("No hay cupos disponibles para esta entrada.");
            }

            // Crear la compra
            var compra = new Compra
            {
                IdUsuario = 1, // Simulado
                IdEvento = dto.IdEvento,
                MetodoPago = "Pendiente",
                EstadoPago = "Pendiente",
                MontoTotal = tipoEntrada.Precio * dto.Cantidad,
                Confirmada = false,
                Reembolsado = false
            };
            _context.Compras.Add(compra);
            await _context.SaveChangesAsync();

            // Crear el detalle
            var detalle = new DetalleCompra
            {
                IdCompra = compra.IdCompra,
                IdTipoEntrada = dto.IdTipoEntrada,
                Cantidad = dto.Cantidad,
                PrecioUnitario = tipoEntrada.Precio
            };
            _context.DetalleCompras.Add(detalle);
            await _context.SaveChangesAsync();

            return Ok(new { IdCompra = compra.IdCompra, MontoTotal = compra.MontoTotal });
        }

        [HttpPost("aplicar-cupon")]
        public async Task<IActionResult> AplicarCupon([FromBody] AplicarCuponDto dto)
        {
            var cupon = await _context.Promocions
                .FirstOrDefaultAsync(p => p.Codigo == dto.Codigo && p.IdEvento == dto.IdEvento && p.Activo == true);

            if (cupon == null)
            {
                return Ok(new ResultadoCuponDto { Valido = false, Mensaje = "El código no es válido para este evento." });
            }

            // Aquí deberías aplicar lógica real de descuento sobre el precio
            return Ok(new ResultadoCuponDto { Valido = true, Mensaje = "Cupón aplicado", Descuento = 20.0m });
        }

        [HttpPost("procesar-pago")]
        public async Task<IActionResult> ProcesarPago([FromBody] ProcesarPagoDto dto)
        {
            var compra = await _context.Compras.FindAsync(dto.IdCompra);
            if (compra == null || compra.EstadoPago != "Pendiente")
            {
                return BadRequest("La orden de compra no es válida o ya fue procesada.");
            }

            // Simulación de pago
            bool pagoExitoso = true;

            if (pagoExitoso)
            {
                compra.EstadoPago = "Aprobado";
                compra.MetodoPago = dto.MetodoPago;
                compra.Confirmada = true;
                compra.CodigoTransaccion = "TX-" + Guid.NewGuid().ToString();
                _context.Entry(compra).State = EntityState.Modified;

                // Generar entradas
                var detalle = await _context.DetalleCompras.FirstAsync(d => d.IdCompra == dto.IdCompra);
                for (int i = 0; i < detalle.Cantidad; i++)
                {
                    var entrada = new Entrada
                    {
                        IdDetalle = detalle.IdDetalle,
                        CodigoQr = "QR-" + Guid.NewGuid().ToString(),
                        Estado = "Activa",
                        FechaEmision = DateTime.Now
                    };
                    _context.Entrada.Add(entrada);
                }

                // Actualizar stock
                var tipoEntrada = await _context.TipoEntrada.FindAsync(detalle.IdTipoEntrada);
                if (tipoEntrada != null)
                {
                    tipoEntrada.CupoDisponible -= detalle.Cantidad;
                    _context.Entry(tipoEntrada).State = EntityState.Modified;
                }

                await _context.SaveChangesAsync();
                return Ok(new { Mensaje = "Pago exitoso. Entradas generadas." });
            }
            else
            {
                compra.EstadoPago = "Rechazado";
                await _context.SaveChangesAsync();
                return BadRequest("El pago fue rechazado.");
            }
        }
    } 

    public class IniciarCompraDto
    {
        [Required]
        public int IdEvento { get; set; }
        [Required]
        public int IdTipoEntrada { get; set; }
        [Range(1, 10)]
        public int Cantidad { get; set; }
    }

    public class AplicarCuponDto
    {
        [Required]
        public string Codigo { get; set; }
        [Required]
        public int IdEvento { get; set; }
    }

    public class ResultadoCuponDto
    {
        public bool Valido { get; set; }
        public decimal NuevoPrecio { get; set; }
        public decimal Descuento { get; set; }
        public string Mensaje { get; set; }
    }

    public class ProcesarPagoDto
    {
        [Required]
        public int IdCompra { get; set; }
        [Required]
        public string MetodoPago { get; set; }
        public string TokenPago { get; set; }
    }
}