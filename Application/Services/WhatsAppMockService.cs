using FutbolComplejo.Application.DTOs;
using FutbolComplejo.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FutbolComplejo.Application.Services;

/// <summary>
/// Mock del servicio de WhatsApp. En producción, reemplazar con la implementación
/// real usando Meta WhatsApp Business API o Twilio.
/// Simula el envío y loguea los mensajes que se enviarían.
/// </summary>
public class WhatsAppMockService : IWhatsAppService
{
    private readonly ILogger<WhatsAppMockService> _logger;

    public WhatsAppMockService(ILogger<WhatsAppMockService> logger)
    {
        _logger = logger;
    }

    public Task<bool> EnviarNotificacionTurnoDisponibleAsync(
        string telefono, string nombreUsuario, string cancha,
        DateTime fechaHora, decimal precio)
    {
        var mensaje = $"""
            📢 *¡TURNO DISPONIBLE!*
            Hola {nombreUsuario}! 👋
            
            Se liberó el turno que estabas esperando:
            ⚽ *{cancha}*
            📅 {fechaHora:dddd dd/MM/yyyy} a las {fechaHora:HH:mm}hs
            💰 ${precio:N0} ARS/hora
            
            ¡Reservá ahora en La Masia antes de que alguien más lo tome!
            👉 [Link al sistema de reservas]
            
            _Este mensaje fue enviado automáticamente. Respondé SI para confirmar._
            """;

        _logger.LogInformation(
            "[WhatsApp MOCK] → {Telefono}\n{Mensaje}", telefono, mensaje);

        // Simular latencia de API real
        return Task.FromResult(true);
    }

    public Task<bool> EnviarConfirmacionReservaAsync(
        string telefono, string nombreUsuario, TurnoDto turno)
    {
        var mensaje = $"""
            ✅ *RESERVA CONFIRMADA*
            Hola {nombreUsuario}!
            
            Tu reserva fue confirmada exitosamente:
            ⚽ *{turno.CanchaNombre}*
            📅 {turno.FechaHoraInicio:dddd dd/MM/yyyy}
            🕐 {turno.FechaHoraInicio:HH:mm} - {turno.FechaHoraFin:HH:mm}hs
            💰 Total: ${turno.PrecioTotal:N0} ARS
            🔖 Nro. Reserva: #{turno.Id}
            
            ¡Te esperamos en La Masia! ⚽🏟️
            """;

        _logger.LogInformation(
            "[WhatsApp MOCK] Confirmación → {Telefono}\n{Mensaje}", telefono, mensaje);

        return Task.FromResult(true);
    }

    public Task<bool> EnviarCancelacionReservaAsync(
        string telefono, string nombreUsuario, TurnoDto turno)
    {
        var mensaje = $"""
            ❌ *RESERVA CANCELADA*
            Hola {nombreUsuario},
            
            Tu reserva fue cancelada:
            ⚽ {turno.CanchaNombre}
            📅 {turno.FechaHoraInicio:dddd dd/MM/yyyy} a las {turno.FechaHoraInicio:HH:mm}hs
            🔖 Nro. Reserva: #{turno.Id}
            
            Si cancelaste por error, podés volver a reservar desde nuestra app.
            Disculpá las molestias.
            """;

        _logger.LogInformation(
            "[WhatsApp MOCK] Cancelación → {Telefono}\n{Mensaje}", telefono, mensaje);

        return Task.FromResult(true);
    }
}
