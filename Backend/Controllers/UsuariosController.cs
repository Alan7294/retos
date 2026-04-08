using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pw2_clase5.Data;
using pw2_clase5.Models;
using pw2_clase5.Services;

namespace pw2_clase5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;

        public UsuariosController(ApplicationDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // GET: api/Usuarios (solo activos)
        [HttpGet]
        [Authorize(Policy = "AdminOrUser")]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios
                .Where(u => u.Activo)
                .ToListAsync();
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOrUser")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null || !usuario.Activo)
            {
                return NotFound();
            }

            return usuario;
        }

        // GET: api/Usuarios/search?nombre=Alan
        [HttpGet("search")]
        [Authorize(Policy = "AdminOrUser")]
        public async Task<ActionResult<IEnumerable<Usuario>>> SearchUsuarios([FromQuery] string nombre)
        {
            var usuarios = await _context.Usuarios
                .Where(u => u.Activo && u.Nombre.Contains(nombre))
                .ToListAsync();

            if (!usuarios.Any())
            {
                return NotFound();
            }

            return usuarios;
        }

        // POST: api/Usuarios
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            // Validar duplicados
            if (await _context.Usuarios.AnyAsync(u => u.Nombre == usuario.Nombre))
            {
                return BadRequest(new { message = "El nombre de usuario ya está en uso." });
            }

            if (await _context.Usuarios.AnyAsync(u => u.Correo == usuario.Correo))
            {
                return BadRequest(new { message = "El correo ya está registrado." });
            }

            usuario.Activo = true; // siempre se crea activo
            usuario.FechaRegistro = DateTime.Now;

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(usuario);
        }

        // PUT: api/Usuarios/5
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            if (id != usuario.IdUsuario)
            {
                return BadRequest();
            }

            _context.Entry(usuario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Usuarios.Any(e => e.IdUsuario == id))
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

        // PUT: api/Usuarios/deshabilitar/5
        [HttpPut("deshabilitar/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeshabilitarUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Activo = false;
            _context.Entry(usuario).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario deshabilitado correctamente", usuario });
        }

        // PUT: api/Usuarios/habilitar/5
        [HttpPut("habilitar/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> HabilitarUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Activo = true;
            _context.Entry(usuario).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario habilitado nuevamente", usuario });
        }

        // POST: api/Usuarios/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Nombre == request.Username && u.Password == request.Password && u.Activo);

            if (usuario == null)
            {
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
            }

            var rol = usuario.Nombre.ToLower().Contains("admin") ? "ADMIN" : "USER";
            var token = _jwtService.GenerateToken(usuario.IdUsuario, usuario.Nombre, rol);

            return Ok(new { 
                message = "Login exitoso", 
                token,
                usuario = new { 
                    id = usuario.IdUsuario,
                    nombre = usuario.Nombre,
                    correo = usuario.Correo,
                    puntajeTotal = usuario.PuntajeTotal
                }
            });
        }

        // POST: api/Usuarios/login-keycloak
        [HttpPost("login-keycloak")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginKeycloak([FromBody] KeycloakLoginRequest request)
        {
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(request.Token) as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;
                
                if (jsonToken == null)
                {
                    return Unauthorized(new { message = "Token de Keycloak inválido" });
                }

                var username = jsonToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value 
                    ?? jsonToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value
                    ?? "user";

                var email = jsonToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? $"{username}@keycloak.local";

                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Nombre == username);

                if (usuario == null)
                {
                    usuario = new Usuario
                    {
                        Nombre = username,
                        Correo = email,
                        Password = "",
                        PuntajeTotal = 0,
                        FechaRegistro = DateTime.Now,
                        Activo = true
                    };
                    _context.Usuarios.Add(usuario);
                    await _context.SaveChangesAsync();
                }

                var roles = jsonToken.Claims
                    .Where(c => c.Type == "realm_access.roles" || c.Type == "roles")
                    .Select(c => c.Value)
                    .ToList();

                var rol = roles.Any(r => r.ToLower().Contains("admin")) ? "ADMIN" : "USER";
                
                if (!usuario.Activo)
                {
                    return Unauthorized(new { message = "Usuario desactivado" });
                }

                var token = _jwtService.GenerateToken(usuario.IdUsuario, usuario.Nombre, rol);

                return Ok(new { 
                    message = "Login exitoso", 
                    token,
                    usuario = new { 
                        id = usuario.IdUsuario,
                        nombre = usuario.Nombre,
                        correo = usuario.Correo,
                        puntajeTotal = usuario.PuntajeTotal
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Keycloak login error: " + ex.Message);
                return Unauthorized(new { message = "Error al validar token de Keycloak" });
            }
        }

        public class KeycloakLoginRequest
        {
            public string Token { get; set; }
        }

        // POST: api/Usuarios/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Nombre == request.Nombre))
            {
                return BadRequest(new { message = "El nombre de usuario ya está en uso." });
            }

            if (await _context.Usuarios.AnyAsync(u => u.Correo == request.Correo))
            {
                return BadRequest(new { message = "El correo ya está registrado." });
            }

            var usuario = new Usuario
            {
                Nombre = request.Nombre,
                Correo = request.Correo,
                Password = request.Password,
                PuntajeTotal = 0,
                FechaRegistro = DateTime.Now,
                Activo = true
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(usuario.IdUsuario, usuario.Nombre, "USER");

            return Ok(new { 
                message = "Registro exitoso",
                token,
                usuario = new {
                    id = usuario.IdUsuario,
                    nombre = usuario.Nombre,
                    correo = usuario.Correo,
                    puntajeTotal = usuario.PuntajeTotal
                }
            });
        }

        public class RegisterRequest
        {
            public string Nombre { get; set; }
            public string Correo { get; set; }
            public string Password { get; set; }
        }

        // GET: api/Usuarios/ranking - Top 10 usuarios por puntaje
        [HttpGet("ranking")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetRanking()
        {
            var ranking = await _context.Usuarios
                .Where(u => u.Activo)
                .OrderByDescending(u => u.PuntajeTotal)
                .Take(10)
                .Select(u => new {
                    u.IdUsuario,
                    u.Nombre,
                    u.PuntajeTotal,
                    RetosCompletados = _context.Soluciones
                        .Count(s => s.IdUsuario == u.IdUsuario && s.Estado == "correcto" && s.Activo)
                })
                .ToListAsync();

            return Ok(ranking);
        }

        // GET: api/Usuarios/{id}/estadisticas
        [HttpGet("{id}/estadisticas")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetEstadisticas(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null || !usuario.Activo)
            {
                return NotFound();
            }

            var estadisticas = new
            {
                usuario.IdUsuario,
                usuario.Nombre,
                usuario.PuntajeTotal,
                usuario.FechaRegistro,
                TotalEnvios = await _context.Soluciones.CountAsync(s => s.IdUsuario == id && s.Activo),
                EnviosCorrectos = await _context.Soluciones.CountAsync(s => s.IdUsuario == id && s.Estado == "correcto" && s.Activo),
                EnviosIncorrectos = await _context.Soluciones.CountAsync(s => s.IdUsuario == id && s.Estado == "incorrecto" && s.Activo),
                EnviosPendientes = await _context.Soluciones.CountAsync(s => s.IdUsuario == id && s.Estado == "pendiente" && s.Activo)
            };

            return Ok(estadisticas);
        }

        // GET: api/Usuarios/{id}/historial
        [HttpGet("{id}/historial")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetHistorial(int id)
        {
            var historial = await _context.Soluciones
                .Include(s => s.Reto)
                .Where(s => s.IdUsuario == id && s.Activo)
                .OrderByDescending(s => s.FechaEnvio)
                .Select(s => new
                {
                    s.IdSolucion,
                    s.IdReto,
                    RetoTitulo = s.Reto.Titulo,
                    s.Estado,
                    s.FechaEnvio,
                    s.Codigo
                })
                .ToListAsync();

            return Ok(historial);
        }

        // GET: api/Usuarios/{id}/retos-completados
        [HttpGet("{id}/retos-completados")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetRetosCompletados(int id)
        {
            var retosCompletados = await _context.Soluciones
                .Include(s => s.Reto)
                .Where(s => s.IdUsuario == id && s.Estado == "correcto" && s.Activo)
                .Select(s => new
                {
                    s.IdReto,
                    s.Reto.Titulo,
                    s.Reto.Dificultad,
                    s.Reto.Puntos,
                    FechaCompletado = s.FechaEnvio
                })
                .ToListAsync();

            return Ok(retosCompletados);
        }

        // Clase auxiliar para recibir datos del login
        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
