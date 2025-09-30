using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ForariaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ForariaConnection"))
);

builder.Services.AddScoped<IClaimRepository, ImplementationClaimRepository>();
builder.Services.AddScoped<CreateClaim>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ForariaContext>();
    context.Database.Migrate();

}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
