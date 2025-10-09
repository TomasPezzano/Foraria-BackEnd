using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using Foraria.Infrastructure.Blockchain;
using Foraria.Infrastructure.Configuration;
using Foraria.Infrastructure.Persistence;
using Foraria.Infrastructure.Repository;
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

builder.Services.AddScoped<IForumRepository, ForumRepository>();
builder.Services.AddScoped<IThreadRepository, ThreadRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IReactionRepository, ReactionRepository>();
builder.Services.AddScoped<IBlockchainProofRepository, BlockchainProofRepository>();

builder.Services.AddScoped<IBlockchainService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();

    var rpc = config["Blockchain:RpcUrl"];
    var pk = config["Blockchain:PrivateKey"];
    var contract = config["Blockchain:ContractAddress"];

    var abiPath = Path.Combine(
        AppContext.BaseDirectory,
        "Infrastructure",
        "Blockchain",
        "ForariaNotary.abi.json"
    );

    var abi = File.ReadAllText(abiPath);

    return new PolygonBlockchainService(rpc, pk, contract, abi);
});


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
