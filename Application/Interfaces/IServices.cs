using FutbolComplejo.Application.DTOs;

namespace FutbolComplejo.Application.Interfaces;

public interface IReservaService
{
    Task<ApiResponse<TurnoDto>> CrearReservaAsync(CrearTurnoDto dto);
    Task<ApiResponse<TurnoDto>> CancelarReservaAsync(CancelarTurnoDto dto);
    Task<ApiResponse<DisponibilidadCanchaDto>> ObtenerDisponibilidadAsync(int canchaId, DateTime fecha);
    Task<ApiResponse<List<TurnoDto>>> ObtenerTurnosPorCanchaAsync(int canchaId, DateTime desde, DateTime hasta);
    Task<ApiResponse<List<TurnoDto>>> ObtenerTurnosPorUsuarioAsync(int usuarioId);
}

public interface IListaEsperaService
{
    Task<ApiResponse<ListaEsperaDto>> RegistrarEnListaEsperaAsync(CrearListaEsperaDto dto);
    Task<ApiResponse<List<ListaEsperaDto>>> ObtenerListaEsperaPorSlotAsync(int canchaId, DateTime fechaHora);
    Task ProcessarNotificacionesAsync(int turnoId); // Llamado cuando un turno se cancela
    Task<ApiResponse<bool>> RemoverDeListaEsperaAsync(int listaEsperaId);
}

public interface IWhatsAppService
{
    Task<bool> EnviarNotificacionTurnoDisponibleAsync(string telefono, string nombreUsuario, string cancha, DateTime fechaHora, decimal precio);
    Task<bool> EnviarConfirmacionReservaAsync(string telefono, string nombreUsuario, TurnoDto turno);
    Task<bool> EnviarCancelacionReservaAsync(string telefono, string nombreUsuario, TurnoDto turno);
}

public interface ICanchaRepository
{
    Task<List<FutbolComplejo.Domain.Entities.Cancha>> ObtenerTodasAsync();
    Task<FutbolComplejo.Domain.Entities.Cancha?> ObtenerPorIdAsync(int id);
}

public interface ITurnoRepository
{
    Task<FutbolComplejo.Domain.Entities.Turno?> ObtenerPorIdAsync(int id);
    Task<List<FutbolComplejo.Domain.Entities.Turno>> ObtenerPorCanchaYFechaAsync(int canchaId, DateTime desde, DateTime hasta);
    Task<bool> ExisteConflictoAsync(int canchaId, DateTime inicio, DateTime fin, int? excluirTurnoId = null);
    Task<FutbolComplejo.Domain.Entities.Turno> AgregarAsync(FutbolComplejo.Domain.Entities.Turno turno);
    Task ActualizarAsync(FutbolComplejo.Domain.Entities.Turno turno);
    Task<List<FutbolComplejo.Domain.Entities.Turno>> ObtenerPorUsuarioAsync(int usuarioId);
}

public interface IListaEsperaRepository
{
    Task<List<FutbolComplejo.Domain.Entities.ListaEspera>> ObtenerPorSlotAsync(int canchaId, DateTime fechaHora);
    Task<FutbolComplejo.Domain.Entities.ListaEspera> AgregarAsync(FutbolComplejo.Domain.Entities.ListaEspera entrada);
    Task ActualizarAsync(FutbolComplejo.Domain.Entities.ListaEspera entrada);
    Task<int> ObtenerSiguientePosicionAsync(int canchaId, DateTime fechaHora);
    Task<FutbolComplejo.Domain.Entities.ListaEspera?> ObtenerPorIdAsync(int id);
}

public interface IUsuarioRepository
{
    Task<FutbolComplejo.Domain.Entities.Usuario?> ObtenerPorTelefonoAsync(string telefono);
    Task<FutbolComplejo.Domain.Entities.Usuario?> ObtenerPorIdAsync(int id);
    Task<FutbolComplejo.Domain.Entities.Usuario> AgregarAsync(FutbolComplejo.Domain.Entities.Usuario usuario);
}
