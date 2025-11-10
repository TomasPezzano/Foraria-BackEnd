using Foraria;
using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using Foraria.Domain.Service;
using Foraria.Hubs;
using Foraria.Infrastructure.Blockchain;
using Foraria.Infrastructure.Email;
using Foraria.Infrastructure.Infrastructure.Persistence;
using Foraria.Infrastructure.Infrastructure.Services;
using Foraria.Infrastructure.Persistence;
using Foraria.Infrastructure.Repository;
using Foraria.SignalRImplementation;
using ForariaDomain.Aplication.Configuration;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using ForariaDomain.Services;
using MercadoPago.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
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
builder.Services.AddScoped<ISupplierContractRepository, SupplierContractRepository>();
builder.Services.AddScoped<ICreateSupplier, CreateSupplier>();
builder.Services.AddScoped<IDeleteSupplier, DeleteSupplier>();
builder.Services.AddScoped<ICreateSupplierContract, CreateSupplierContract>();
builder.Services.AddScoped<ILocalFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IContractExpirationService, ContractExpirationService>();
builder.Services.AddScoped<IGetAllSupplier, GetAllSupplier>();
builder.Services.AddHostedService<PollExpirationService>();
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
builder.Services.AddScoped<ISignalRNotification, SignalRNotification>();
builder.Services.AddScoped<GetSupplierById>();
builder.Services.AddScoped<GetSupplierContractById>();
builder.Services.AddScoped<GetSupplierContractsById>();
//builder.Services.AddScoped<GetMonthlyExpenseTotal>();
//builder.Services.AddScoped<GetExpenseByCategory>();
builder.Services.AddScoped<GetActivePollCount>();
//builder.Services.AddScoped<GetPendingExpenses>();
//builder.Services.AddScoped<GetUserExpenseSummary>();
//builder.Services.AddScoped<GetUserMonthlyExpenseHistory>();
builder.Services.AddScoped<GetTotalUsers>();
builder.Services.AddScoped<GetLatestPendingClaim>();
builder.Services.AddScoped<GetPendingClaimsCount>();    
//builder.Services.AddScoped<GetCollectedExpensesPercentage>();
builder.Services.AddScoped<GetUpcomingReserves>();
builder.Services.AddScoped<GetForumWithThreads>();
builder.Services.AddScoped<DeleteForum>();
builder.Services.AddScoped<DeleteThread>();
builder.Services.AddScoped<GetAllThreads>();
builder.Services.AddScoped<CloseThread>();
builder.Services.AddScoped<GetThreadWithMessages>();
builder.Services.AddScoped<UpdateThread>();
builder.Services.AddScoped<GetMessagesByUser>();
builder.Services.AddScoped<UpdateMessage>();
builder.Services.AddScoped<HideMessage>();
builder.Services.AddScoped<IForumRepository, ForumRepository>();
builder.Services.AddScoped<IThreadRepository, ThreadRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IReactionRepository, ReactionRepository>();
builder.Services.AddScoped<IBlockchainProofRepository, BlockchainProofRepository>();
builder.Services.AddScoped<IBlockchainService, PolygonBlockchainService>();
builder.Services.AddScoped<GetPollWithResults>();
builder.Services.AddScoped<GetAllPollsWithResults>();
builder.Services.AddScoped<GetAllPollsWithResults>();
builder.Services.Configure<BlockchainSettings>(
    builder.Configuration.GetSection("Blockchain"));
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IReserveRepository, ReserveRepository>();
builder.Services.AddScoped<ICreateReserve, CreateReserve>();
builder.Services.AddScoped<IGetAllReserve, GetAllReserve>();
builder.Services.AddScoped<IUpdateOldReserves, UpdateOldReserves>();
builder.Services.AddHostedService<OldReserveBackgroundService>();
builder.Services.AddScoped<IGetUserByEmail, GetUserByEmail>();
builder.Services.AddScoped<IGetUserById, GetUserById>();
builder.Services.AddScoped<IGetResponsibleSectorById, GetResponsibleSectorById>();
builder.Services.AddScoped<IGetClaimById, GetClaimById>();
builder.Services.AddScoped<GetForumWithCategory>();
builder.Services.AddScoped<GetThreadCommentCount>();
builder.Services.AddScoped<IOcrService, AzureOcrService>();
builder.Services.AddScoped<IProcessInvoiceOcr, ProcessInvoiceOcr>();
builder.Services.AddScoped<IFileProcessor, FileProcessor>();
builder.Services.AddScoped<IGetPlaceById, GetPlaceById>();
builder.Services.AddScoped<IPlaceRepository, PlaceRepository>();
builder.Services.AddScoped<GetActiveReserveCount>();
builder.Services.AddScoped<ICreateInvoice, CreateInvoice>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IGetAllInvoices, GetAllInvoices>();
builder.Services.AddScoped<IGetConsortiumById, GetConsortiumById>();
builder.Services.AddScoped<IConsortiumRepository, ConsortiumRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, MercadoPagoService>();
builder.Services.AddScoped<CreatePreferenceMP>();
builder.Services.AddScoped<ProcessWebHookMP>();
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
builder.Services.AddScoped<IGetTotalTenantUsers, GetTotalTenantUsers>();
builder.Services.AddScoped<IGetTotalOwnerUsers, GetTotalOwnerUsers>();
builder.Services.AddScoped<IGetUsersByConsortium, GetUsersByConsortium>();
builder.Services.AddScoped<IGetResidenceById, GetResidenceById>();
builder.Services.AddScoped<IGetAllResidencesByConsortium, GetAllResidencesByConsortium>();
builder.Services.AddScoped<ITransferPermission, TransferPermission>();
builder.Services.AddScoped<IRevokePermission, RevokePermission>();
builder.Services.AddScoped<ICreateExpense, CreateExpense>();
//builder.Services.AddScoped<IGetAllInvoicesByMonth, GetAllInvoicesByMonth>();
builder.Services.AddScoped<IGetAllInvoicesByMonthAndConsortium, GetAllInvoicesByMonthAndConsortium>();
builder.Services.AddScoped<IGetAllExpenses, GetAllExpenses>();
builder.Services.AddScoped<ICreateExpenseDetail, CreateExpenseDetail>();
builder.Services.AddScoped<IExpenseDetailRepository, ExpenseDatailRepository>();
builder.Services.AddScoped<IGetAllResidencesByConsortiumWithOwner, GetAllResidencesByConsortiumWithOwner>();
builder.Services.AddScoped<IGetExpenseWithDto, GetExpenseWithDto>();
builder.Services.AddScoped<IGetExpenseDetailByResidence, GetExpenseDetailByResidence>();
builder.Services.AddScoped<GetLastUploadDate>();
builder.Services.AddScoped<UpdatePoll>();
builder.Services.AddScoped<ChangePollState>();
builder.Services.AddScoped<GetUserDocumentsByCategory>();
builder.Services.AddScoped<GetUserDocumentStats>();
builder.Services.AddScoped<IPasswordResetTokenGenerator, PasswordResetTokenGenerator>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddScoped<IForgotPassword, ForgotPassword>();
builder.Services.AddScoped<IResetPassword, ResetPassword>();


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
            ClockSkew = TimeSpan.Zero,

            RoleClaimType = ClaimTypes.Role
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
    options.AddPolicy("OwnerAndTenant", policy =>
        policy.RequireRole("Propietario", "Inquilino"));
    options.AddPolicy("All", policy =>
        policy.RequireRole("Consorcio", "Administrador", "Propietario", "Inquilino"));


    options.AddPolicy("CanVote", policy =>
    policy.RequireAssertion(context =>
    {
        var role = context.User.FindFirst(ClaimTypes.Role)?.Value;
        var hasPermissionClaim = context.User.FindFirst("hasPermission")?.Value;
        var hasPermission = hasPermissionClaim == "True";

        if (role == "Consorcio" || role == "Administrador" || role == "Propietario")
            return true;

        if (role == "Inquilino" && hasPermission)
            return true;

        return false;
    }));

    options.AddPolicy("CanAttendMeetings", policy =>
        policy.RequireAssertion(context =>
        {
            var role = context.User.FindFirst(ClaimTypes.Role)?.Value;
            var hasPermissionClaim = context.User.FindFirst("hasPermission")?.Value;
            var hasPermission = hasPermissionClaim == "True";

            if (role == "Consorcio" || role == "Administrador" || role == "Propietario")
                return true;

            if (role == "Inquilino" && hasPermission)
                return true;

            return false;
        }));
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Foraria API",
        Version = "v1",
        Description = "API Foraria."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer.\r\n\r\n" +
                      "Ingresá tu token así:\r\n\r\n" +
                      "Bearer {tu_token_jwt}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

builder.Services.AddDbContext<ForariaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ForariaConnection"))
);

builder.Services.AddScoped<IPollRepository, PollRepositoryImplementation>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<CreatePoll>();
builder.Services.AddScoped<IVoteRepository, VoteRepositoryImplementation>();
builder.Services.AddScoped<CreateVote>();
builder.Services.AddScoped<GetPolls>();

MercadoPagoConfig.AccessToken = builder.Configuration["MercadoPago:AccessToken"];
var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

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

app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "uploads")),
    RequestPath = "/uploads"
});

app.MapHub<PollHub>("/pollHub");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();