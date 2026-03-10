using System.ComponentModel.DataAnnotations.Schema;
using FutbolComplejo.Domain.Enums;

namespace FutbolComplejo.Domain.Entities;

/// <summary>
/// Representa una reserva/turno para una cancha en fecha y hora específica.
/// </summary>
public class Turno
{
    public int Id { get; set; }

    public int CanchaId { get; set; }
    public Cancha Cancha { get; set; } = null!;

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    /// <summary>Fecha y hora de inicio del turno.</summary>
    public DateTime FechaHoraInicio { get; set; }

    /// <summary>Fecha y hora de fin del turno (generalmente 1 hora después).</summary>
    public DateTime FechaHoraFin { get; set; }

    public EstadoReserva Estado { get; set; } = EstadoReserva.Pendiente;

    /// <summary>Precio total cobrado en ARS al momento de la reserva.</summary>
    public decimal PrecioTotal { get; set; }

    /// <summary>Nombre del equipo o referencia del grupo.</summary>
    public string? NombreEquipo { get; set; }

    public string? NotasAdicionales { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaCancelacion { get; set; }

    public string? MotivoCancelacion { get; set; }

    /// <summary>
    /// True si el turno fue cancelado. Activa la lógica de notificación de lista de espera.
    /// [NotMapped] evita que EF Core intente persistir esta propiedad calculada.
    /// </summary>
    [NotMapped]
    public bool IsCancelled => Estado == EstadoReserva.Cancelada;

    // Navigation
    public ICollection<ListaEspera> ListasEspera { get; set; } = new List<ListaEspera>();
}
