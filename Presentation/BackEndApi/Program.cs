using Microsoft.OpenApi.Models;
using ASPNET.BackEnd;

var builder = WebApplication.CreateBuilder(args);

// Register all backend services (including MediatR, Application, Infrastructure, etc.)
builder.Services.AddBackEndServices(builder.Configuration);

var app = builder.Build();

// Register backend middleware, Swagger, and database seeding
app.RegisterBackEndBuilder(app.Environment, app, builder.Configuration);

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
