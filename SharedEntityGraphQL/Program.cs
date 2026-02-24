using Microsoft.EntityFrameworkCore;
using SharedEntityGraphQL.Data;
using SharedEntityGraphQL.Queries;
using SharedEntityGraphQL.Mutations;
using SharedEntityGraphQL.Types;
using SharedEntityGraphQL.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Entity Framework Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()
    )
);

// Add services to the container
builder.Services.AddScoped<StateService>();

// Add GraphQL Services
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddObjectType<StateType>();

// Add CORS if needed
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Map root endpoint to redirect to GraphQL
app.MapGet("/", () => Results.Redirect("/graphql", permanent: false));

// Map GraphQL endpoint
app.MapGraphQL();

app.Run();
