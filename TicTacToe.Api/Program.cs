using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<GameDbContext>(options=>options.UseSqlite("Data Source=tictactoe.db"));
builder.Services.AddCors(options=>options.AddPolicy("AllowFrontend",policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
WebApplication app = builder.Build();
app.UseCors("AllowFrontend");
app.MapControllers();
app.Run();