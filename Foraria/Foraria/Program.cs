using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using Foraria.Domain.Service;
using Foraria.Hubs;
using Foraria.Infrastructure.Blockchain;
using Foraria.Infrastructure.Email;
using Foraria.Infrastructure.Infrastructure.Persistence;
using Foraria.Infrastructure.Persistence;
using Foraria.Infrastructure.Repository;
using Foraria.SignalRImplementation;
using ForariaDomain.Aplication.Configuration;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
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
builder.Services.AddScoped<ISendEmail, SmtpEmailService>();
builder.Services.AddScoped<ICreateResidence, CreateResidence>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<ILoginUser, LoginUser>();
builder.Services.AddScoped<ILogoutUser, LogoutUser>();
builder.Services.AddScoped<IRefreshTokenUseCase, RefreshToken>();
builder.Services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<ICreateSupplier, CreateSupplier>();
builder.Services.AddScoped<IDeleteSupplier, DeleteSupplier>();


builder.Services.AddScoped<IClaimRepository, ClaimImplementation>();
builder.Services.AddScoped<ICreateClaim, CreateClaim>();
builder.Services.AddScoped<IGetClaims, GetClaims>();
builder.Services.AddScoped<IRejectClaim, RejectClaim>();

builder.Services.AddScoped<IClaimResponseRepository, ClaimResponseImplementation>();
builder.Services.AddScoped<ICreateClaimResponse, CreateClaimResponse>();

builder.Services.AddScoped<IResponsibleSectorRepository, ResponsibleSectorImplementation>();

builder.Services.AddScoped<IUserDocumentRepository, UserDocumentImplementation>();
builder.Services.AddScoped<ICreateUserDocument, CreateUserDocument>();
builder.Services.AddScoped<IGetUserDocuments, GetUserDocuments>();

builder.Services.AddScoped<CreateForum>();
builder.Services.AddScoped<CreateThread>();
builder.Services.AddScoped<CreateMessage>();
builder.Services.AddScoped<GetAllForums>();
builder.Services.AddScoped<GetForumById>();
builder.Services.AddScoped<GetMessageById>();
builder.Services.AddScoped<GetMessagesByThread>();
builder.Services.AddScoped<GetThreadById>();
builder.Services.AddScoped<ToggleReaction>();
builder.Services.AddScoped<DeleteMessage>();
builder.Services.AddScoped<NotarizePoll>();
builder.Services.AddScoped<GetPollById>();
builder.Services.AddScoped<ISignalRNotification, SignalRNotification>();


builder.Services.AddScoped<IForumRepository, ForumRepository>();
builder.Services.AddScoped<IThreadRepository, ThreadRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IReactionRepository, ReactionRepository>();
builder.Services.AddScoped<IBlockchainProofRepository, BlockchainProofRepository>();
builder.Services.AddScoped<IBlockchainService, PolygonBlockchainService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddSignalR();

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

app.UseCors("AllowReactApp");

app.MapHub<PollHub>("/pollHub");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
