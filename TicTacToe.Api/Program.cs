WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<Dictionary<Guid,Game>>();
WebApplication app = builder.Build();
app.MapControllers();
app.Run();