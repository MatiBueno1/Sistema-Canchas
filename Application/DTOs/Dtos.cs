using FutbolComplejo.Domain.Enums;

namespace FutbolComplejo.Application.DTOs;

// ─────────────────────────────────────────
// CANCHA
// ─────────────────────────────────────────
public record CanchaDto(
    int Id,
    string Nombre,
    string Descripcion,
    TipoCancha Tipo,
    decimal PrecioPorHora,
    int CapacidadJugadores,
    bool EstaActiva
);

// ─────────────────────────────────────────
// USUARIO
// ─────────────────────────────────────────
public record UsuarioDto(
    int Id,
    string NombreCompleto,
    string Telefono,
    string? Email
);

public record CrearUsuarioDto(
    string NombreCompleto,
    string Telefono,
    string? Email
);

// ─────────────────────────────────────────
// TURNO / RESERVA
// ─────────────────────────────────────────
public record TurnoDto(
    int Id,
    int CanchaId,
    string CanchaNombre,
    int UsuarioId,
    string UsuarioNombre,
    string UsuarioTelefono,
    DateTime FechaHoraInicio,
    DateTime FechaHoraFin,
    EstadoReserva Estado,
    decimal PrecioTotal,
    string? NombreEquipo,
    bool IsCancelled
);

public record CrearTurnoDto(
    int CanchaId,
    DateTime FechaHoraInicio,
    /// <summary>Duración en horas. Default 1.</summary>
    int DuracionHoras,
    string NombreCompleto,
    string Telefono,
    string? Email,
    string? NombreEquipo,
    string? NotasAdicionales
);

public record CancelarTurnoDto(
    int TurnoId,
    string MotivoCancelacion
);

// ─────────────────────────────────────────
// DISPONIBILIDAD
// ─────────────────────────────────────────

/// <summary>Slot horario con su estado para un día dado.</summary>
public record SlotDisponibleDto(
    DateTime FechaHoraInicio,
    DateTime FechaHoraFin,
    bool EstaDisponible,
    decimal Precio,
    int? TurnoId
);

public record DisponibilidadCanchaDto(
    int CanchaId,
    string CanchaNombre,
    DateTime Fecha,
    List<SlotDisponibleDto> Slots
);

// ─────────────────────────────────────────
// LISTA DE ESPERA
// ─────────────────────────────────────────
public record ListaEsperaDto(
    int Id,
    int CanchaId,
    string CanchaNombre,
    int UsuarioId,
    string UsuarioNombre,
    string TelefonoWhatsApp,
    DateTime FechaHoraDeseada,
    int Posicion,
    bool FueNotificado,
    bool Confirmo
);

public record CrearListaEsperaDto(
    int CanchaId,
    DateTime FechaHoraDeseada,
    string NombreCompleto,
    string TelefonoWhatsApp,
    string? Email
);

// ─────────────────────────────────────────
// RESPUESTA GENÉRICA
// ─────────────────────────────────────────
public record ApiResponse<T>(
    bool Success,
    string? Message,
    T? Data,
    List<string>? Errors = null
);
