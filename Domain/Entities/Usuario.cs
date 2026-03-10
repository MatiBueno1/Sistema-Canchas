namespace FutbolComplejo.Domain.Entities;

/// <summary>
/// Usuario que realiza reservas o se anota en lista de espera.
/// </summary>
public class Usuario
{
    public int Id { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>Número de WhatsApp con código de país. Ej: +5491112345678</summary>
    public string Telefono { get; set; } = string.Empty;

    public string? Email { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
    public ICollection<ListaEspera> ListasEspera { get; set; } = new List<ListaEspera>();
}
