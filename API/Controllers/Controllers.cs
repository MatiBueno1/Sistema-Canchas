using FutbolComplejo.Application.DTOs;
using FutbolComplejo.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FutbolComplejo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CanchasController : ControllerBase
{
    private readonly ICanchaRepository _canchaRepo;

    public CanchasController(ICanchaRepository canchaRepo) => _canchaRepo = canchaRepo;

    /// <summary>Obtiene todas las canchas activas.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var canchas = await _canchaRepo.ObtenerTodasAsync();
        var dtos = canchas.Select(c => new CanchaDto(
            c.Id, c.Nombre, c.Descripcion, c.Tipo, c.PrecioPorHora, c.CapacidadJugadores, c.EstaActiva
        )).ToList();
        return Ok(new ApiResponse<List<CanchaDto>>(true, null, dtos));
    }

    /// <summary>Obtiene una cancha por ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var c = await _canchaRepo.ObtenerPorIdAsync(id);
        if (c is null) return NotFound(new ApiResponse<CanchaDto>(false, "Cancha no encontrada.", null));
        return Ok(new ApiResponse<CanchaDto>(true, null, new CanchaDto(
            c.Id, c.Nombre, c.Descripcion, c.Tipo, c.PrecioPorHora, c.CapacidadJugadores, c.EstaActiva)));
    }
}

// ─────────────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReservasController : ControllerBase
{
    private readonly IReservaService _reservaService;

    public ReservasController(IReservaService reservaService) => _reservaService = reservaService;

    /// <summary>Crea una nueva reserva.</summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearTurnoDto dto)
    {
        var result = await _reservaService.CrearReservaAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Cancela una reserva existente.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancelar(int id, [FromBody] string motivo)
    {
        var result = await _reservaService.CancelarReservaAsync(new CancelarTurnoDto(id, motivo));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Obtiene disponibilidad de una cancha para una fecha específica.</summary>
    [HttpGet("disponibilidad/{canchaId:int}")]
    public async Task<IActionResult> Disponibilidad(int canchaId, [FromQuery] DateTime fecha)
    {
        var result = await _reservaService.ObtenerDisponibilidadAsync(canchaId, fecha);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Lista turnos de una cancha en un rango de fechas.</summary>
    [HttpGet("cancha/{canchaId:int}")]
    public async Task<IActionResult> PorCancha(int canchaId, [FromQuery] DateTime desde, [FromQuery] DateTime hasta)
    {
        var result = await _reservaService.ObtenerTurnosPorCanchaAsync(canchaId, desde, hasta);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Lista todos los turnos de un usuario.</summary>
    [HttpGet("usuario/{usuarioId:int}")]
    public async Task<IActionResult> PorUsuario(int usuarioId)
    {
        var result = await _reservaService.ObtenerTurnosPorUsuarioAsync(usuarioId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

// ─────────────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ListaEsperaController : ControllerBase
{
    private readonly IListaEsperaService _listaService;

    public ListaEsperaController(IListaEsperaService listaService) => _listaService = listaService;

    /// <summary>Registra un usuario en la lista de espera para un slot.</summary>
    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] CrearListaEsperaDto dto)
    {
        var result = await _listaService.RegistrarEnListaEsperaAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Obtiene la lista de espera para un slot específico.</summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerPorSlot([FromQuery] int canchaId, [FromQuery] DateTime fechaHora)
    {
        var result = await _listaService.ObtenerListaEsperaPorSlotAsync(canchaId, fechaHora);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Remueve a un usuario de la lista de espera.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        var result = await _listaService.RemoverDeListaEsperaAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Autenticación del panel de administración.
/// Las credenciales se leen desde appsettings.json — nunca están en el JS del cliente.
/// </summary>
[ApiController]
[Route("api/admin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IConfiguration _config;

    public AdminController(IConfiguration config) => _config = config;

    public record LoginRequest(string Usuario, string Password);
    public record LoginResponse(bool Success, string? Token, string? Error);

    /// <summary>Verifica las credenciales del admin y devuelve un token de sesión simple.</summary>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        var adminUser = _config["Admin:Usuario"] ?? "admin";
        var adminPass = _config["Admin:Password"] ?? "lamasia2024";

        if (req.Usuario == adminUser && req.Password == adminPass)
        {
            // Token simple basado en fecha — en producción usar JWT real
            var token = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes($"admin:{DateTime.UtcNow:yyyyMMdd}")
            );
            return Ok(new LoginResponse(true, token, null));
        }

        // Delay de 1 segundo para dificultar ataques de fuerza bruta
        System.Threading.Thread.Sleep(1000);
        return Unauthorized(new LoginResponse(false, null, "Credenciales incorrectas."));
    }
}
