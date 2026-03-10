namespace FutbolComplejo.Domain.Entities;

/// <summary>
/// Registro en lista de espera para un turno ocupado.
/// Cuando el turno se cancela, se notifica por WhatsApp en orden de inscripción.
/// </summary>
public class ListaEspera
{
    public int Id { get; set; }

    /// <summary>
    /// Referencia al turno OCUPADO que el usuario quiere tomar si se libera.
    /// Se guarda la combinación CanchaId + FechaHoraInicio para poder buscar
    /// incluso si el Turno original aún no existe (reserva directa vs slot).
    /// </summary>
    public int? TurnoId { get; set; }
    public Turno? Turno { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    /// <summary>Cancha deseada.</summary>
    public int CanchaId { get; set; }
    public Cancha Cancha { get; set; } = null!;

    /// <summary>Slot de tiempo deseado.</summary>
    public DateTime FechaHoraDeseada { get; set; }

    /// <summary>Número de WhatsApp para notificar (puede diferir del usuario registrado).</summary>
    public string TelefonoWhatsApp { get; set; } = string.Empty;

    /// <summary>Posición en la cola (1 = primero en ser notificado).</summary>
    public int Posicion { get; set; }

    public bool FueNotificado { get; set; } = false;

    public DateTime? FechaNotificacion { get; set; }

    /// <summary>True si el usuario confirmó que tomará el turno luego de la notificación.</summary>
    public bool Confirmo { get; set; } = false;

    public DateTime FechaInscripcion { get; set; } = DateTime.UtcNow;

    public bool EstaActivo { get; set; } = true;
}
