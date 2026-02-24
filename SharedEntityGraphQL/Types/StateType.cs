using HotChocolate.Types;
using SharedEntityGraphQL.Models;

namespace SharedEntityGraphQL.Types;

public class StateType : ObjectType<State>
{
    protected override void Configure(IObjectTypeDescriptor<State> descriptor)
    {
        descriptor
            .Description("Represents a US state");

        descriptor
            .Field(f => f.Id)
            .Description("The unique identifier for the state");

        descriptor
            .Field(f => f.Name)
            .Description("The name of the state");

        descriptor
            .Field(f => f.Abbreviation)
            .Description("The two-letter abbreviation of the state");

        descriptor
            .Field(f => f.CreatedAt)
            .Description("The date and time when the state was created");
    }
}
