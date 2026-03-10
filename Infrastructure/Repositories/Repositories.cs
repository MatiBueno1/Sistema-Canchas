using FutbolComplejo.Application.Interfaces;
using FutbolComplejo.Domain.Entities;
using FutbolComplejo.Domain.Enums;
using FutbolComplejo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FutbolComplejo.Infrastructure.Repositories;

public class CanchaRepository : ICanchaRepository
{
    private readonly FutbolComplejoDbContext _ctx;
    public CanchaRepository(FutbolComplejoDbContext ctx) => _ctx = ctx;

    public Task<List<Cancha>> ObtenerTodasAsync() =>
        _ctx.Canchas.Where(c => c.EstaActiva).AsNoTracking().ToListAsync();

    public Task<Cancha?> ObtenerPorIdAsync(int id) =>
        _ctx.Canchas.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
}

public class UsuarioRepository : IUsuarioRepository
{
    private readonly FutbolComplejoDbContext _ctx;
    public UsuarioRepository(FutbolComplejoDbContext ctx) => _ctx = ctx;

    public Task<Usuario?> ObtenerPorTelefonoAsync(string telefono) =>
        _ctx.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Telefono == telefono);

    public Task<Usuario?> ObtenerPorIdAsync(int id) =>
        _ctx.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

    public async Task<Usuario> AgregarAsync(Usuario usuario)
    {
        _ctx.Usuarios.Add(usuario);
        await _ctx.SaveChangesAsync();
        return usuario;
    }
}

public class TurnoRepository : ITurnoRepository
{
    private readonly FutbolComplejoDbContext _ctx;
    public TurnoRepository(FutbolComplejoDbContext ctx) => _ctx = ctx;

    public Task<Turno?> ObtenerPorIdAsync(int id) =>
        _ctx.Turnos.FirstOrDefaultAsync(t => t.Id == id);

    public Task<List<Turno>> ObtenerPorCanchaYFechaAsync(int canchaId, DateTime desde, DateTime hasta) =>
        _ctx.Turnos
            .Where(t => t.CanchaId == canchaId
                     && t.FechaHoraInicio < hasta
                     && t.FechaHoraFin > desde
                     && t.Estado != EstadoReserva.Cancelada)
            .AsNoTracking()
            .ToListAsync();

    public async Task<bool> ExisteConflictoAsync(int canchaId, DateTime inicio, DateTime fin, int? excluirTurnoId = null)
    {
        var query = _ctx.Turnos
            .Where(t => t.CanchaId == canchaId
                     && t.Estado != EstadoReserva.Cancelada
                     && t.FechaHoraInicio < fin
                     && t.FechaHoraFin > inicio);

        if (excluirTurnoId.HasValue)
            query = query.Where(t => t.Id != excluirTurnoId.Value);

        return await query.AnyAsync();
    }

    public async Task<Turno> AgregarAsync(Turno turno)
    {
        _ctx.Turnos.Add(turno);
        await _ctx.SaveChangesAsync();
        return turno;
    }

    public Task ActualizarAsync(Turno turno)
    {
        _ctx.Turnos.Update(turno);
        return _ctx.SaveChangesAsync();
    }

    public Task<List<Turno>> ObtenerPorUsuarioAsync(int usuarioId) =>
        _ctx.Turnos
            .Where(t => t.UsuarioId == usuarioId)
            .OrderByDescending(t => t.FechaHoraInicio)
            .AsNoTracking()
            .ToListAsync();
}

public class ListaEsperaRepository : IListaEsperaRepository
{
    private readonly FutbolComplejoDbContext _ctx;
    public ListaEsperaRepository(FutbolComplejoDbContext ctx) => _ctx = ctx;

    public Task<List<ListaEspera>> ObtenerPorSlotAsync(int canchaId, DateTime fechaHora) =>
        _ctx.ListasEspera
            .Where(l => l.CanchaId == canchaId && l.FechaHoraDeseada == fechaHora)
            .OrderBy(l => l.Posicion)
            .ToListAsync();

    public async Task<ListaEspera> AgregarAsync(ListaEspera entrada)
    {
        _ctx.ListasEspera.Add(entrada);
        await _ctx.SaveChangesAsync();
        return entrada;
    }

    public Task ActualizarAsync(ListaEspera entrada)
    {
        _ctx.ListasEspera.Update(entrada);
        return _ctx.SaveChangesAsync();
    }

    public async Task<int> ObtenerSiguientePosicionAsync(int canchaId, DateTime fechaHora)
    {
        var max = await _ctx.ListasEspera
            .Where(l => l.CanchaId == canchaId && l.FechaHoraDeseada == fechaHora && l.EstaActivo)
            .MaxAsync(l => (int?)l.Posicion) ?? 0;
        return max + 1;
    }

    public Task<ListaEspera?> ObtenerPorIdAsync(int id) =>
        _ctx.ListasEspera.FirstOrDefaultAsync(l => l.Id == id);
}
