using FutbolComplejo.Application.DTOs;
using FutbolComplejo.Application.Interfaces;
using FutbolComplejo.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FutbolComplejo.Application.Services;

/// <summary>
/// Gestiona la lista de espera inteligente.
/// Cuando un turno se cancela, notifica en orden a los usuarios inscritos
/// hasta que uno confirme o se agote la lista.
/// </summary>
public class ListaEsperaService : IListaEsperaService
{
    private readonly IListaEsperaRepository _listaRepo;
    private readonly ITurnoRepository _turnoRepo;
    private readonly ICanchaRepository _canchaRepo;
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IWhatsAppService _whatsAppService;
    private readonly ILogger<ListaEsperaService> _logger;

    public ListaEsperaService(
        IListaEsperaRepository listaRepo,
        ITurnoRepository turnoRepo,
        ICanchaRepository canchaRepo,
        IUsuarioRepository usuarioRepo,
        IWhatsAppService whatsAppService,
        ILogger<ListaEsperaService> logger)
    {
        _listaRepo = listaRepo;
        _turnoRepo = turnoRepo;
        _canchaRepo = canchaRepo;
        _usuarioRepo = usuarioRepo;
        _whatsAppService = whatsAppService;
        _logger = logger;
    }

    public async Task<ApiResponse<ListaEsperaDto>> RegistrarEnListaEsperaAsync(CrearListaEsperaDto dto)
    {
        var cancha = await _canchaRepo.ObtenerPorIdAsync(dto.CanchaId);
        if (cancha is null)
            return Fail<ListaEsperaDto>("La cancha no existe.");

        // Obtener o crear usuario
        var usuario = await _usuarioRepo.ObtenerPorTelefonoAsync(dto.TelefonoWhatsApp)
                      ?? await _usuarioRepo.AgregarAsync(new Usuario
                      {
                          NombreCompleto = dto.NombreCompleto,
                          Telefono = dto.TelefonoWhatsApp,
                          Email = dto.Email
                      });

        // Verificar que no esté ya en la lista para ese slot
        var listaActual = await _listaRepo.ObtenerPorSlotAsync(dto.CanchaId, dto.FechaHoraDeseada);
        if (listaActual.Any(l => l.UsuarioId == usuario.Id && l.EstaActivo))
            return Fail<ListaEsperaDto>("Ya estás registrado en la lista de espera para este horario.");

        var posicion = await _listaRepo.ObtenerSiguientePosicionAsync(dto.CanchaId, dto.FechaHoraDeseada);

        var entrada = new ListaEspera
        {
            CanchaId = dto.CanchaId,
            UsuarioId = usuario.Id,
            FechaHoraDeseada = dto.FechaHoraDeseada,
            TelefonoWhatsApp = dto.TelefonoWhatsApp,
            Posicion = posicion,
            EstaActivo = true
        };

        entrada = await _listaRepo.AgregarAsync(entrada);
        _logger.LogInformation("Usuario {UsuarioId} registrado en lista de espera pos {Pos} para cancha {CanchaId} - {Hora}",
            usuario.Id, posicion, dto.CanchaId, dto.FechaHoraDeseada);

        return Ok(MapDto(entrada, cancha.Nombre, usuario.NombreCompleto),
            $"Registrado en lista de espera. Tu posición es #{posicion}. Te notificaremos por WhatsApp si el turno se libera.");
    }

    public async Task<ApiResponse<List<ListaEsperaDto>>> ObtenerListaEsperaPorSlotAsync(int canchaId, DateTime fechaHora)
    {
        var lista = await _listaRepo.ObtenerPorSlotAsync(canchaId, fechaHora);
        var cancha = await _canchaRepo.ObtenerPorIdAsync(canchaId);
        if (cancha is null) return Fail<List<ListaEsperaDto>>("Cancha no encontrada.");

        var dtos = new List<ListaEsperaDto>();
        foreach (var l in lista.Where(x => x.EstaActivo).OrderBy(x => x.Posicion))
        {
            var usr = await _usuarioRepo.ObtenerPorIdAsync(l.UsuarioId);
            dtos.Add(MapDto(l, cancha.Nombre, usr?.NombreCompleto ?? "Desconocido"));
        }
        return Ok(dtos);
    }

    /// <summary>
    /// Llamado automáticamente cuando un turno se cancela (IsCancelled = true).
    /// Notifica al primero de la cola que no fue notificado aún.
    /// </summary>
    public async Task ProcessarNotificacionesAsync(int turnoId)
    {
        var turno = await _turnoRepo.ObtenerPorIdAsync(turnoId);
        if (turno is null) return;

        var lista = await _listaRepo.ObtenerPorSlotAsync(turno.CanchaId, turno.FechaHoraInicio);
        var pendientes = lista
            .Where(l => l.EstaActivo && !l.FueNotificado)
            .OrderBy(l => l.Posicion)
            .ToList();

        if (!pendientes.Any())
        {
            _logger.LogInformation("No hay usuarios en lista de espera para el turno {TurnoId}.", turnoId);
            return;
        }

        var cancha = await _canchaRepo.ObtenerPorIdAsync(turno.CanchaId);

        // Notificar al primero de la lista
        var primero = pendientes.First();
        var usuario = await _usuarioRepo.ObtenerPorIdAsync(primero.UsuarioId);

        if (usuario is null) return;

        var enviado = await _whatsAppService.EnviarNotificacionTurnoDisponibleAsync(
            primero.TelefonoWhatsApp,
            usuario.NombreCompleto,
            cancha?.Nombre ?? "Cancha",
            turno.FechaHoraInicio,
            cancha?.PrecioPorHora ?? 0
        );

        if (enviado)
        {
            primero.FueNotificado = true;
            primero.FechaNotificacion = DateTime.UtcNow;
            await _listaRepo.ActualizarAsync(primero);
            _logger.LogInformation("Notificación enviada a usuario {UsuarioId} (pos {Pos}) para turno liberado {TurnoId}.",
                usuario.Id, primero.Posicion, turnoId);
        }
    }

    public async Task<ApiResponse<bool>> RemoverDeListaEsperaAsync(int listaEsperaId)
    {
        var entrada = await _listaRepo.ObtenerPorIdAsync(listaEsperaId);
        if (entrada is null) return Fail<bool>("Entrada no encontrada.");

        entrada.EstaActivo = false;
        await _listaRepo.ActualizarAsync(entrada);
        return Ok(true, "Removido de la lista de espera.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    private static ListaEsperaDto MapDto(ListaEspera l, string canchaNombre, string usuarioNombre) =>
        new(l.Id, l.CanchaId, canchaNombre, l.UsuarioId, usuarioNombre,
            l.TelefonoWhatsApp, l.FechaHoraDeseada, l.Posicion, l.FueNotificado, l.Confirmo);

    private static ApiResponse<T> Ok<T>(T data, string? msg = null) => new(true, msg, data);
    private static ApiResponse<T> Fail<T>(string msg) => new(false, msg, default);
}
