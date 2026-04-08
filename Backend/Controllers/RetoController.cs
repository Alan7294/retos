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
    public class RetosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public RetosController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Retos (solo activos, con filtros opcionales)
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Reto>>> GetRetos(
            [FromQuery] string? dificultad = null,
            [FromQuery] string? ordenarPor = null)
        {
            var query = _context.Retos.Where(r => r.Activo);

            if (!string.IsNullOrEmpty(dificultad))
            {
                query = query.Where(r => r.Dificultad.ToLower() == dificultad.ToLower());
            }

            query = ordenarPor?.ToLower() switch
            {
                "puntos" => query.OrderByDescending(r => r.Puntos),
                "fecha" => query.OrderByDescending(r => r.FechaCreacion),
                "titulo" => query.OrderBy(r => r.Titulo),
                _ => query.OrderByDescending(r => r.FechaCreacion)
            };

            return await query.ToListAsync();
        }

        // GET: api/Retos/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Reto>> GetReto(int id)
        {
            var reto = await _context.Retos.FindAsync(id);

            if (reto == null || !reto.Activo)
            {
                return NotFound();
            }

            return reto;
        }

        // GET: api/Retos/con-estado/{idUsuario} - Retos con marca de completado
        [HttpGet("con-estado/{idUsuario}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetRetosConEstado(int idUsuario, 
            [FromQuery] string? dificultad = null,
            [FromQuery] string? ordenarPor = null)
        {
            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null || !usuario.Activo)
            {
                return BadRequest(new { message = "Usuario no válido" });
            }

            var retosCompletados = await _context.Soluciones
                .Where(s => s.IdUsuario == idUsuario && s.Estado == "correcto" && s.Activo)
                .Select(s => s.IdReto)
                .ToListAsync();

            var query = _context.Retos.Where(r => r.Activo);

            if (!string.IsNullOrEmpty(dificultad))
            {
                query = query.Where(r => r.Dificultad.ToLower() == dificultad.ToLower());
            }

            query = ordenarPor?.ToLower() switch
            {
                "puntos" => query.OrderByDescending(r => r.Puntos),
                "fecha" => query.OrderByDescending(r => r.FechaCreacion),
                "titulo" => query.OrderBy(r => r.Titulo),
                _ => query.OrderByDescending(r => r.FechaCreacion)
            };

            var retos = await query.ToListAsync();

            var resultado = retos.Select(r => new {
                r.IdReto,
                r.Titulo,
                r.Descripcion,
                r.Dificultad,
                r.Puntos,
                r.FechaCreacion,
                Completado = retosCompletados.Contains(r.IdReto)
            });

            return Ok(resultado);
        }

        // GET: api/Retos/search?titulo=algo
        [HttpGet("search")]
        [Authorize(Policy = "AdminOrUser")]
        public async Task<ActionResult<IEnumerable<Reto>>> SearchRetos([FromQuery] string titulo)
        {
            var retos = await _context.Retos
                .Where(r => r.Activo && r.Titulo.Contains(titulo))
                .ToListAsync();

            if (!retos.Any())
            {
                return NotFound();
            }

            return retos;
        }

        // POST: api/Retos
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<Reto>> PostReto([FromBody] CrearRetoRequest request)
        {
            var usuario = await _context.Usuarios.FindAsync(request.IdUsuario);
            if (usuario == null || !usuario.Activo)
            {
                return BadRequest(new { message = "Usuario no válido" });
            }

            if (!usuario.Nombre.ToLower().Contains("admin"))
            {
                return Unauthorized(new { message = "Solo los administradores pueden crear retos" });
            }

            var reto = new Reto
            {
                Titulo = request.Titulo,
                Descripcion = request.Descripcion,
                Dificultad = request.Dificultad,
                Puntos = request.Puntos,
                FechaCreacion = DateTime.Now,
                Activo = true
            };

            _context.Retos.Add(reto);
            await _context.SaveChangesAsync();
            return Ok(reto);
        }

        public class CrearRetoRequest
        {
            public int IdUsuario { get; set; }
            public string Titulo { get; set; }
            public string Descripcion { get; set; }
            public string Dificultad { get; set; }
            public int Puntos { get; set; }
        }

        // PUT: api/Retos/5
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> PutReto(int id, Reto reto)
        {
            if (id != reto.IdReto)
            {
                return BadRequest();
            }

            _context.Entry(reto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Retos.Any(e => e.IdReto == id))
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

        // DELETE: api/Retos/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteReto(int id)
        {
            var reto = await _context.Retos.FindAsync(id);
            if (reto == null)
            {
                return NotFound();
            }

            _context.Retos.Remove(reto);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Reto eliminado correctamente" });
        }

        // PUT: api/Retos/deshabilitar/5
        [HttpPut("deshabilitar/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeshabilitarReto(int id)
        {
            var reto = await _context.Retos.FindAsync(id);
            if (reto == null)
            {
                return NotFound();
            }

            reto.Activo = false;
            _context.Entry(reto).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Reto deshabilitado correctamente", reto });
        }

        // PUT: api/Retos/habilitar/5
        [HttpPut("habilitar/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> HabilitarReto(int id)
        {
            var reto = await _context.Retos.FindAsync(id);
            if (reto == null)
            {
                return NotFound();
            }

            reto.Activo = true;
            _context.Entry(reto).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Reto habilitado nuevamente", reto });
        }
    }
}
