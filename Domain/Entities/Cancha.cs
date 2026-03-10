using FutbolComplejo.Domain.Enums;

namespace FutbolComplejo.Domain.Entities;

/// <summary>
/// Representa una cancha de fútbol en el complejo.
/// </summary>
public class Cancha
{
    public int Id { get; set; }

    /// <summary>Nombre descriptivo. Ej: "Cancha 1 - Fútbol 7"</summary>
    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public TipoCancha Tipo { get; set; }

    /// <summary>Precio por hora en ARS.</summary>
    public decimal PrecioPorHora { get; set; }

    /// <summary>Capacidad máxima de jugadores (10 para F5, 14 para F7).</summary>
    public int CapacidadJugadores { get; set; }

    public bool EstaActiva { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
}
