using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiPelicula.DTOs.Factura;

namespace WebApiPelicula.Controllers
{
    [ApiController]
    [Route("api/facturas")]
    public class FacturasController: ControllerBase
    {
        private readonly ApplicationDbContext context;

        public FacturasController(ApplicationDbContext context)
        {
            this.context = context;
        }
        [HttpPost]
        public async Task<ActionResult> Pagar(PagarFacturaDTO pagarFacturaDTO)
        {
            var facturaDB = await context.Factura
                                //.Include(x => x.Usuario)
                                .FirstOrDefaultAsync(x => x.Id == pagarFacturaDTO.FacturaId);

            if (facturaDB == null)
            {
                return NotFound();
            }

            if (facturaDB.Pagada)
            {
                return BadRequest("La factura ya fue saldada");
            }

            // Lógica para pagar la factura
            // nosotros vamos a pretender que el pago fue exitoso

            facturaDB.Pagada = true;
            await context.SaveChangesAsync();

            var hayFacturasPendientesVencidas = await context.Factura
                .AnyAsync(x => x.UsuarioId == facturaDB.UsuarioId &&
                !x.Pagada && x.FechaLimiteDePago < DateTime.Today);

            if (!hayFacturasPendientesVencidas)
            {
                //facturaDB.Usuario.MalaPaga = false;
                await context.SaveChangesAsync();
            }

            return NoContent();
        }
    }
}
