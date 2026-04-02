using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<GameDbContext>(options=>options.UseSqlite("Data Source=tictactoe.db"));
WebApplication app = builder.Build();
app.MapControllers();
app.Run();