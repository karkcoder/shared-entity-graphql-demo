using SharedEntityGraphQL.Models;
using SharedEntityGraphQL.Services;

namespace SharedEntityGraphQL.Mutations;

public class Mutation
{
    [GraphQLDescription("Creates a new state")]
    public async Task<State> CreateState(string name, string abbreviation, StateService stateService)
    {
        return await stateService.CreateStateAsync(name, abbreviation);
    }

    [GraphQLDescription("Updates an existing state")]
    public async Task<State?> UpdateState(
        Guid id, 
        string? name = null, 
        string? abbreviation = null, 
        StateService? stateService = null)
    {
        return await stateService!.UpdateStateAsync(id, name, abbreviation);
    }

    [GraphQLDescription("Deletes a state by its ID")]
    public async Task<bool> DeleteState(Guid id, StateService stateService)
    {
        return await stateService.DeleteStateAsync(id);
    }
}
