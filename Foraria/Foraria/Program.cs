using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Infrastructure.Configuration;
using Foraria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// Add services to the container.

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IResidenceRepository, ResidenceRepository>();

builder.Services.AddScoped<IRegisterUser, RegisterUser>();
builder.Services.AddScoped<IGeneratePassword, GeneratePassword>();
builder.Services.AddScoped<IPasswordHash, PasswordHash>();
builder.Services.AddScoped<ISendEmail, SendEmail>();
builder.Services.AddScoped<ICreateResidence, CreateResidence>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<ILoginUser, LoginUser>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ForariaContext>(options=>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ForariaConnection"))
);

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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
