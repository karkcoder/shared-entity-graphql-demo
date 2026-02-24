using Microsoft.EntityFrameworkCore;
using SharedEntityGraphQL.Data;
using SharedEntityGraphQL.Models;

namespace SharedEntityGraphQL.Services;

public class StateService
{
    private readonly ApplicationDbContext _context;

    public StateService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<State>> GetAllStatesAsync()
    {
        return await _context.States.OrderBy(s => s.Name).ToListAsync();
    }

    public async Task<State?> GetStateByIdAsync(Guid id)
    {
        return await _context.States.FindAsync(id);
    }

    public async Task<State?> GetStateByAbbreviationAsync(string abbreviation)
    {
        return await _context.States
            .FirstOrDefaultAsync(s => s.Abbreviation == abbreviation);
    }

    public async Task<State> CreateStateAsync(string name, string abbreviation)
    {
        var state = new State
        {
            Id = Guid.NewGuid(),
            Name = name,
            Abbreviation = abbreviation,
            CreatedAt = DateTime.UtcNow
        };

        _context.States.Add(state);
        await _context.SaveChangesAsync();
        return state;
    }

    public async Task<State?> UpdateStateAsync(Guid id, string? name, string? abbreviation)
    {
        var state = await _context.States.FindAsync(id);
        if (state == null)
            return null;

        if (!string.IsNullOrEmpty(name))
            state.Name = name;

        if (!string.IsNullOrEmpty(abbreviation))
            state.Abbreviation = abbreviation;

        await _context.SaveChangesAsync();
        return state;
    }

    public async Task<bool> DeleteStateAsync(Guid id)
    {
        var state = await _context.States.FindAsync(id);
        if (state == null)
            return false;

        _context.States.Remove(state);
        await _context.SaveChangesAsync();
        return true;
    }
}
