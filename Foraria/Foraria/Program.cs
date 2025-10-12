using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using Foraria.Domain.Service;
using Foraria.Infrastructure.Blockchain;
using Foraria.Infrastructure.Email;
using Foraria.Infrastructure.Infrastructure.Persistence;
using Foraria.Infrastructure.Persistence;
using Foraria.Infrastructure.Repository;
using ForariaDomain.Aplication.Configuration;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using ForariaDomain.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
builder.Services.AddScoped<IRefreshTokenUseCase, RefreshTokenUseCase>();
builder.Services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<ICreateSupplier, CreateSupplier>();
builder.Services.AddScoped<IDeleteSupplier, DeleteSupplier>();
builder.Services.AddScoped<ICreateSupplierContract, CreateSupplierContract>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IContractExpirationService, ContractExpirationService>();
builder.Services.AddScoped<IGetAllSupplier, GetAllSupplier>();
builder.Services.AddScoped<IUpdateUserFirstTime, UpdateUserFirstTime>();



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
builder.Services.AddScoped<GetMonthlyExpenseTotal>();
builder.Services.AddScoped<GetExpenseByCategory>();
builder.Services.AddScoped<GetActivePollCount>();
builder.Services.AddScoped<GetPendingExpenses>();
builder.Services.AddScoped<GetUserExpenseSummary>();
builder.Services.AddScoped<GetUserMonthlyExpenseHistory>();
builder.Services.AddScoped<GetTotalUsers>();
builder.Services.AddScoped<GetLatestPendingClaim>();
builder.Services.AddScoped<GetPendingClaimsCount>();    
builder.Services.AddScoped<GetCollectedExpensesPercentage>();
builder.Services.AddScoped<GetUpcomingReserves>();


builder.Services.AddScoped<IForumRepository, ForumRepository>();
builder.Services.AddScoped<IThreadRepository, ThreadRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IReactionRepository, ReactionRepository>();
builder.Services.AddScoped<IBlockchainProofRepository, BlockchainProofRepository>();
builder.Services.AddScoped<IBlockchainService, PolygonBlockchainService>();
builder.Services.Configure<BlockchainSettings>(
    builder.Configuration.GetSection("Blockchain"));
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IReserveRepository, ReserveRepository>();



builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

    });

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey)
            ),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OnlyConsortium", policy =>
        policy.RequireRole("Consorcio"));
    options.AddPolicy("ConsortiumAndAdmin", policy =>
        policy.RequireRole("Consorcio", "Administrador"));
    options.AddPolicy("Owner", policy =>
        policy.RequireRole("Propietario"));
    options.AddPolicy("Tenant", policy =>
        policy.RequireRole("Inquilino"));
    options.AddPolicy("All", policy =>
        policy.RequireRole("Consorcio", "Administrador", "Popietario", "Inquilino"));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ForariaContext>(options =>
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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
