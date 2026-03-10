using FutbolComplejo.Application.DTOs;
using FutbolComplejo.Application.Interfaces;
using FutbolComplejo.Domain.Entities;
using FutbolComplejo.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FutbolComplejo.Application.Services;

/// <summary>
/// Servicio principal de reservas. Contiene toda la lógica de negocio:
/// validación de horarios habilitados, detección de conflictos,
/// cálculo de precios y orquestación de notificaciones.
/// </summary>
public class ReservaService : IReservaService
{
    private readonly ITurnoRepository _turnoRepo;
    private readonly ICanchaRepository _canchaRepo;
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IListaEsperaService _listaEsperaService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly ILogger<ReservaService> _logger;

    // ── Horarios habilitados por tipo de día ──────────────────────────────
    // Lun–Jue y Vie: 16:00 – 01:00 (+1 día)
    // Sáb y Dom:     16:00 – 02:00 (+1 día)
    private static readonly TimeSpan HoraApertura = new(16, 0, 0);
    private static readonly TimeSpan HorrarioCierre_LunViernes = new(1, 0, 0);   // 01:00 del día siguiente
    private static readonly TimeSpan HorarioCierre_SabDom = new(2, 0, 0);        // 02:00 del día siguiente

    public ReservaService(
        ITurnoRepository turnoRepo,
        ICanchaRepository canchaRepo,
        IUsuarioRepository usuarioRepo,
        IListaEsperaService listaEsperaService,
        IWhatsAppService whatsAppService,
        ILogger<ReservaService> logger)
    {
        _turnoRepo = turnoRepo;
        _canchaRepo = canchaRepo;
        _usuarioRepo = usuarioRepo;
        _listaEsperaService = listaEsperaService;
        _whatsAppService = whatsAppService;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────────────
    public async Task<ApiResponse<TurnoDto>> CrearReservaAsync(CrearTurnoDto dto)
    {
        // 1. Validar cancha existente
        var cancha = await _canchaRepo.ObtenerPorIdAsync(dto.CanchaId);
        if (cancha is null || !cancha.EstaActiva)
            return Fail<TurnoDto>("La cancha no existe o no está disponible.");

        // 2. Validar duración
        if (dto.DuracionHoras < 1 || dto.DuracionHoras > 4)
            return Fail<TurnoDto>("La duración debe ser entre 1 y 4 horas.");

        var inicio = dto.FechaHoraInicio;
        var fin = inicio.AddHours(dto.DuracionHoras);

        // 3. Validar que los minutos sean :00 (no medias horas)
        if (inicio.Minute != 0)
            return Fail<TurnoDto>("Los turnos deben comenzar en punto (ej: 16:00, 17:00).");

        // 4. Validar horario habilitado
        var errHorario = ValidarHorarioHabilitado(inicio, fin);
        if (errHorario is not null)
            return Fail<TurnoDto>(errHorario);

        // 5. Verificar disponibilidad (sin solapamiento)
        var hayConflicto = await _turnoRepo.ExisteConflictoAsync(dto.CanchaId, inicio, fin);
        if (hayConflicto)
            return Fail<TurnoDto>("El horario seleccionado ya está reservado. Podés anotarte en la lista de espera.");

        // 6. Obtener o crear usuario
        var usuario = await _usuarioRepo.ObtenerPorTelefonoAsync(dto.Telefono)
                      ?? await _usuarioRepo.AgregarAsync(new Usuario
                      {
                          NombreCompleto = dto.NombreCompleto,
                          Telefono = dto.Telefono,
                          Email = dto.Email
                      });

        // 7. Calcular precio
        var precio = cancha.PrecioPorHora * dto.DuracionHoras;

        // 8. Crear turno
        var turno = new Turno
        {
            CanchaId = dto.CanchaId,
            UsuarioId = usuario.Id,
            FechaHoraInicio = inicio,
            FechaHoraFin = fin,
            Estado = EstadoReserva.Confirmada,
            PrecioTotal = precio,
            NombreEquipo = dto.NombreEquipo,
            NotasAdicionales = dto.NotasAdicionales
        };

        turno = await _turnoRepo.AgregarAsync(turno);
        _logger.LogInformation("Turno {TurnoId} creado para cancha {CanchaId} el {Inicio}", turno.Id, cancha.Id, inicio);

        var turnoDto = MapTurnoDto(turno, cancha, usuario);

        // 9. Notificar confirmación por WhatsApp (fire & forget)
        _ = _whatsAppService.EnviarConfirmacionReservaAsync(usuario.Telefono, usuario.NombreCompleto, turnoDto);

        return Ok(turnoDto, "Reserva confirmada exitosamente.");
    }

    // ─────────────────────────────────────────────────────────────────────
    public async Task<ApiResponse<TurnoDto>> CancelarReservaAsync(CancelarTurnoDto dto)
    {
        var turno = await _turnoRepo.ObtenerPorIdAsync(dto.TurnoId);
        if (turno is null)
            return Fail<TurnoDto>("El turno no existe.");

        if (turno.IsCancelled)
            return Fail<TurnoDto>("El turno ya fue cancelado.");

        if (turno.FechaHoraInicio <= DateTime.UtcNow.AddHours(-3)) // AR = UTC-3
            return Fail<TurnoDto>("No se puede cancelar un turno que ya comenzó.");

        turno.Estado = EstadoReserva.Cancelada;
        turno.FechaCancelacion = DateTime.UtcNow;
        turno.MotivoCancelacion = dto.MotivoCancelacion;
        await _turnoRepo.ActualizarAsync(turno);

        _logger.LogInformation("Turno {TurnoId} cancelado.", turno.Id);

        var cancha = await _canchaRepo.ObtenerPorIdAsync(turno.CanchaId);
        var usuario = await _usuarioRepo.ObtenerPorIdAsync(turno.UsuarioId);
        var turnoDto = MapTurnoDto(turno, cancha!, usuario!);

        // Notificar cancelación al titular
        _ = _whatsAppService.EnviarCancelacionReservaAsync(usuario!.Telefono, usuario.NombreCompleto, turnoDto);

        // ── DISPARAR LÓGICA DE LISTA DE ESPERA ──
        await _listaEsperaService.ProcessarNotificacionesAsync(turno.Id);

        return Ok(turnoDto, "Turno cancelado. Se notificó a los usuarios en lista de espera.");
    }

    // ─────────────────────────────────────────────────────────────────────
    public async Task<ApiResponse<DisponibilidadCanchaDto>> ObtenerDisponibilidadAsync(int canchaId, DateTime fecha)
    {
        var cancha = await _canchaRepo.ObtenerPorIdAsync(canchaId);
        if (cancha is null)
            return Fail<DisponibilidadCanchaDto>("La cancha no existe.");

        var fechaSolo = fecha.Date;
        var desde = fechaSolo.Add(HoraApertura);
        var esSabDom = fechaSolo.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        var cierreOffset = esSabDom ? HorarioCierre_SabDom : HorrarioCierre_LunViernes;
        var hasta = fechaSolo.AddDays(1).Add(cierreOffset); // el cierre es al día siguiente

        var turnosDelDia = await _turnoRepo.ObtenerPorCanchaYFechaAsync(canchaId, desde, hasta);

        var slots = new List<SlotDisponibleDto>();
        var cursor = desde;

        while (cursor.Add(TimeSpan.FromHours(1)) <= hasta)
        {
            var slotFin = cursor.AddHours(1);
            var turnoOcupante = turnosDelDia.FirstOrDefault(t =>
                t.FechaHoraInicio < slotFin && t.FechaHoraFin > cursor && !t.IsCancelled);

            slots.Add(new SlotDisponibleDto(
                cursor,
                slotFin,
                turnoOcupante is null,
                cancha.PrecioPorHora,
                turnoOcupante?.Id
            ));
            cursor = cursor.AddHours(1);
        }

        return Ok(new DisponibilidadCanchaDto(canchaId, cancha.Nombre, fechaSolo, slots));
    }

    // ─────────────────────────────────────────────────────────────────────
    public async Task<ApiResponse<List<TurnoDto>>> ObtenerTurnosPorCanchaAsync(int canchaId, DateTime desde, DateTime hasta)
    {
        var cancha = await _canchaRepo.ObtenerPorIdAsync(canchaId);
        if (cancha is null) return Fail<List<TurnoDto>>("La cancha no existe.");

        var turnos = await _turnoRepo.ObtenerPorCanchaYFechaAsync(canchaId, desde, hasta);
        var usuarios = new Dictionary<int, Usuario>();

        var dtos = new List<TurnoDto>();
        foreach (var t in turnos)
        {
            if (!usuarios.TryGetValue(t.UsuarioId, out var usr))
            {
                usr = (await _usuarioRepo.ObtenerPorIdAsync(t.UsuarioId))!;
                usuarios[t.UsuarioId] = usr;
            }
            dtos.Add(MapTurnoDto(t, cancha, usr));
        }

        return Ok(dtos);
    }

    public async Task<ApiResponse<List<TurnoDto>>> ObtenerTurnosPorUsuarioAsync(int usuarioId)
    {
        var turnos = await _turnoRepo.ObtenerPorUsuarioAsync(usuarioId);
        var usuario = await _usuarioRepo.ObtenerPorIdAsync(usuarioId);
        if (usuario is null) return Fail<List<TurnoDto>>("Usuario no encontrado.");

        var canchas = new Dictionary<int, Domain.Entities.Cancha>();
        var dtos = new List<TurnoDto>();
        foreach (var t in turnos)
        {
            if (!canchas.TryGetValue(t.CanchaId, out var c))
            {
                c = (await _canchaRepo.ObtenerPorIdAsync(t.CanchaId))!;
                canchas[t.CanchaId] = c;
            }
            dtos.Add(MapTurnoDto(t, c, usuario));
        }
        return Ok(dtos);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Verifica que inicio y fin caigan dentro de los horarios habilitados.
    /// Horario: Apertura 16:00; Cierre 01:00 (Lun-Vie) / 02:00 (Sáb-Dom) del día siguiente.
    /// </summary>
    private static string? ValidarHorarioHabilitado(DateTime inicio, DateTime fin)
    {
        var diaBase = inicio.Date;
        var apertura = diaBase.Add(HoraApertura);

        var esSabDom = diaBase.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        var cierreOffset = esSabDom ? HorarioCierre_SabDom : HorrarioCierre_LunViernes;
        var cierre = diaBase.AddDays(1).Add(cierreOffset);

        if (inicio < apertura)
            return $"El complejo La Masia abre a las 16:00hs.";

        if (fin > cierre)
            return $"La Masia cierra a las {(esSabDom ? "02:00" : "01:00")}hs del día siguiente.";

        return null;
    }

    private static TurnoDto MapTurnoDto(Turno t, Domain.Entities.Cancha c, Usuario u) =>
        new(t.Id, c.Id, c.Nombre, u.Id, u.NombreCompleto, u.Telefono,
            t.FechaHoraInicio, t.FechaHoraFin, t.Estado, t.PrecioTotal,
            t.NombreEquipo, t.IsCancelled);

    private static ApiResponse<T> Ok<T>(T data, string? msg = null) =>
        new(true, msg, data);

    private static ApiResponse<T> Fail<T>(string msg) =>
        new(false, msg, default);
}
