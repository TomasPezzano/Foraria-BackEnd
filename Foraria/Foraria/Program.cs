using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ForariaContext>(options=>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ForariaConnection"))
);

builder.Services.AddScoped<IPollRepository, PollRepositoryImplementation>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<CreatePoll>();
builder.Services.AddScoped<IVoteRepository, VoteRepositoryImplementation>();
builder.Services.AddScoped<CreateVote>();
builder.Services.AddScoped<GetPolls>();
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
