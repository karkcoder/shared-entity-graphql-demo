using SharedEntityGraphQL.Models;
using SharedEntityGraphQL.Services;

namespace SharedEntityGraphQL.Queries;

public class Query
{
    [GraphQLDescription("Gets all states")]
    public async Task<IEnumerable<State>> GetStates(StateService stateService)
    {
        return await stateService.GetAllStatesAsync();
    }

    [GraphQLDescription("Gets a state by its ID")]
    public async Task<State?> GetStateById(Guid id, StateService stateService)
    {
        return await stateService.GetStateByIdAsync(id);
    }

    [GraphQLDescription("Gets a state by its abbreviation")]
    public async Task<State?> GetStateByAbbreviation(string abbreviation, StateService stateService)
    {
        return await stateService.GetStateByAbbreviationAsync(abbreviation);
    }
}
