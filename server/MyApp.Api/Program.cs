using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();

// ✅ CORS for React frontend (http://localhost:3000)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Use CORS
app.UseCors("AllowReact");

// 🔽 Test PostgreSQL connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

try
{
    using var conn = new NpgsqlConnection(connectionString);
    conn.Open();

    Console.WriteLine("✅ Connected to PostgreSQL!");

    using var cmd = new NpgsqlCommand("SELECT version()", conn);
    var version = cmd.ExecuteScalar();

    Console.WriteLine($"PostgreSQL version: {version}");
}
catch (Exception ex)
{
    Console.WriteLine("❌ Database connection failed");
    Console.WriteLine(ex.Message);
}

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 🌟 Endpoint: /users → React frontend can fetch users
app.MapGet("/users", async () =>
{
    var users = new List<object>();

    await using var conn = new NpgsqlConnection(connectionString);
    await conn.OpenAsync();

    await using var cmd = new NpgsqlCommand("SELECT id, name FROM users", conn);
    await using var reader = await cmd.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        users.Add(new
        {
            id = reader.GetInt32(0),
            name = reader.GetString(1)
        });
    }

    return Results.Ok(users);
});

// Existing /weatherforecast endpoint
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm",
    "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

// Record for WeatherForecast
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
