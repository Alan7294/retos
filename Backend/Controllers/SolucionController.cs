using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pw2_clase5.Data;
using pw2_clase5.Models;

namespace pw2_clase5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SolucionesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SolucionesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Soluciones (solo activas)
        [HttpGet]
        [Authorize(Policy = "AdminOrUser")]
        public async Task<ActionResult<IEnumerable<Solucion>>> GetSoluciones()
        {
            return await _context.Soluciones
                .Include(s => s.Usuario)
                .Include(s => s.Reto)
                .Where(s => s.Activo)
                .ToListAsync();
        }

        // GET: api/Soluciones/5
        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOrUser")]
        public async Task<ActionResult<Solucion>> GetSolucion(int id)
        {
            var solucion = await _context.Soluciones
                .Include(s => s.Usuario)
                .Include(s => s.Reto)
                .FirstOrDefaultAsync(s => s.IdSolucion == id && s.Activo);

            if (solucion == null)
            {
                return NotFound();
            }

            return solucion;
        }

        // GET: api/Soluciones/search?descripcion=algo
        [HttpGet("search")]
        [Authorize(Policy = "AdminOrUser")]
        public async Task<ActionResult<IEnumerable<Solucion>>> SearchSoluciones([FromQuery] string descripcion)
        {
            var soluciones = await _context.Soluciones
                .Include(s => s.Usuario)
                .Include(s => s.Reto)
                .Where(s => s.Activo && s.Descripcion.Contains(descripcion))
                .ToListAsync();

            if (!soluciones.Any())
            {
                return NotFound();
            }

            return soluciones;
        }

        // GET: api/Soluciones/por-reto/{idReto} - Todas las soluciones de un reto (Admin)
        [HttpGet("por-reto/{idReto}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetSolucionesPorReto(int idReto)
        {
            var soluciones = await _context.Soluciones
                .Include(s => s.Usuario)
                .Include(s => s.Reto)
                .Where(s => s.IdReto == idReto && s.Activo)
                .OrderByDescending(s => s.FechaEnvio)
                .Select(s => new {
                    s.IdSolucion,
                    s.Codigo,
                    s.Descripcion,
                    s.Estado,
                    s.FechaEnvio,
                    UsuarioNombre = s.Usuario.Nombre,
                    RetoTitulo = s.Reto.Titulo
                })
                .ToListAsync();

            return Ok(soluciones);
        }

        // GET: api/Soluciones/todas - Todas las soluciones (Admin)
        [HttpGet("todas")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetTodasLasSoluciones()
        {
            var soluciones = await _context.Soluciones
                .Include(s => s.Usuario)
                .Include(s => s.Reto)
                .Where(s => s.Activo)
                .OrderByDescending(s => s.FechaEnvio)
                .Select(s => new {
                    s.IdSolucion,
                    s.Codigo,
                    s.Descripcion,
                    s.Estado,
                    s.FechaEnvio,
                    UsuarioId = s.Usuario.IdUsuario,
                    UsuarioNombre = s.Usuario.Nombre,
                    RetoId = s.Reto.IdReto,
                    RetoTitulo = s.Reto.Titulo
                })
                .ToListAsync();

            return Ok(soluciones);
        }

        // POST: api/Soluciones
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<Solucion>> PostSolucion(Solucion solucion)
        {
            var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == solucion.IdUsuario && u.Activo);
            var retoExiste = await _context.Retos.AnyAsync(r => r.IdReto == solucion.IdReto && r.Activo);

            if (!usuarioExiste || !retoExiste)
                return BadRequest("Usuario o Reto no existen o están deshabilitados.");

            solucion.Activo = true;
            _context.Soluciones.Add(solucion);
            await _context.SaveChangesAsync();
            return Ok(solucion);
        }

        // POST: api/Soluciones/enviar - Envío de solución por usuario
        [HttpPost("enviar")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> EnviarSolucion([FromBody] EnvioSolucionRequest request)
        {
            var usuario = await _context.Usuarios.FindAsync(request.IdUsuario);
            if (usuario == null || !usuario.Activo)
            {
                return BadRequest(new { message = "Usuario no válido" });
            }

            var reto = await _context.Retos.FindAsync(request.IdReto);
            if (reto == null || !reto.Activo)
            {
                return BadRequest(new { message = "Reto no válido" });
            }

            var yaCompleto = await _context.Soluciones
                .AnyAsync(s => s.IdUsuario == request.IdUsuario && 
                               s.IdReto == request.IdReto && 
                               s.Estado == "correcto" && 
                               s.Activo);

            if (yaCompleto)
            {
                return BadRequest(new { message = "Ya has completado este reto. No puedes enviar más soluciones." });
            }

            var solucion = new Solucion
            {
                Codigo = request.Codigo,
                Descripcion = request.Descripcion ?? "",
                Estado = "pendiente",
                FechaEnvio = DateTime.Now,
                IdUsuario = request.IdUsuario,
                IdReto = request.IdReto,
                Activo = true
            };

            _context.Soluciones.Add(solucion);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Solución enviada. Estado: pendiente. Esperando evaluación.",
                idSolucion = solucion.IdSolucion,
                estado = solucion.Estado
            });
        }

        // POST: api/Soluciones/evaluar/{id} - Evaluar solución (Admin)
        [HttpPost("evaluar/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<object>> EvaluarSolucion(int id, [FromBody] EvaluarSolucionRequest request)
        {
            var solucion = await _context.Soluciones
                .Include(s => s.Reto)
                .Include(s => s.Usuario)
                .FirstOrDefaultAsync(s => s.IdSolucion == id && s.Activo);

            if (solucion == null)
            {
                return NotFound(new { message = "Solución no encontrada" });
            }

            solucion.Estado = request.Estado;

            if (request.Estado == "correcto")
            {
                var yaAcerto = await _context.Soluciones
                    .AnyAsync(s => s.IdUsuario == solucion.IdUsuario && 
                                   s.IdReto == solucion.IdReto && 
                                   s.Estado == "correcto" && 
                                   s.IdSolucion != id && 
                                   s.Activo);

                if (!yaAcerto)
                {
                    solucion.Usuario.PuntajeTotal += solucion.Reto.Puntos;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Solución evaluada",
                estado = solucion.Estado,
                puntosOtorgados = request.Estado == "correcto" ? solucion.Reto.Puntos : 0
            });
        }

        public class EnvioSolucionRequest
        {
            public int IdUsuario { get; set; }
            public int IdReto { get; set; }
            public string Codigo { get; set; }
            public string? Descripcion { get; set; }
        }

        public class EvaluarSolucionRequest
        {
            public string Estado { get; set; }
        }

        // PUT: api/Soluciones/5
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> PutSolucion(int id, Solucion solucion)
        {
            if (id != solucion.IdSolucion)
            {
                return BadRequest();
            }

            _context.Entry(solucion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Soluciones.Any(e => e.IdSolucion == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PUT: api/Soluciones/deshabilitar/5
        [HttpPut("deshabilitar/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeshabilitarSolucion(int id)
        {
            var solucion = await _context.Soluciones.FindAsync(id);
            if (solucion == null)
            {
                return NotFound();
            }

            solucion.Activo = false;
            _context.Entry(solucion).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Soluci�n deshabilitada correctamente", solucion });
        }

        // PUT: api/Soluciones/habilitar/5
        [HttpPut("habilitar/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> HabilitarSolucion(int id)
        {
            var solucion = await _context.Soluciones.FindAsync(id);
            if (solucion == null)
            {
                return NotFound();
            }

            solucion.Activo = true;
            _context.Entry(solucion).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Soluci�n habilitada nuevamente", solucion });
        }
    }
}
