using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.JsonWebTokens;

using RealEstate.Data;
using RealEstate.Domain;
using RealEstate.Service.JwtService;
using RealEstate.Services;
using RealEstate.Repository.UserRepository;
using RealEstate.Service.UserService;
using RealEstate.Repository.ListingCaseRepository;
using RealEstate.Service.ListingCaseService;
using RealEstate.Repository.MediaAssetRepository;
using RealEstate.Service.MediaAssetService;
using RealEstate.Repository.OrderRepository;
using RealEstate.Service.OrderService;
using RealEstate.Repository.StatusHistoryRepository;
using RealEstate.Service.LoggerService;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Text.Json.Serialization;
using RealEstate.Repository.CaseContactRepository;
using RealEstate.Service.CaseContactService;
using RealEstate.Service.Email;
using Azure.Storage.Blobs;
using RealEstate.Service.AzureBlobStorage;


using RealEstate.Repository.PhotographyCompanyRepository;
using RealEstate.Service.PhotographyCompanyService;





var builder = WebApplication.CreateBuilder(args);

// Load configuration
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Identity configuration
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredUniqueChars = 0;
});

builder.Services.AddIdentityApiEndpoints<User>()
    .AddRoles<Role>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddRoleManager<RoleManager<Role>>()
    .AddDefaultTokenProviders();

// JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))
    };
});

builder.Services.AddAuthorization();

// Swagger setup
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RealEstate API",
        Version = "v1",
        Description = "Real Estate Management API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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
                }
            },
            new string[] {}
        }
    });
});

// EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("RealEstate"));
});

// MongoDB
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<IMongoDbContext, MongoDbContext>();

// CORS
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowVercel", policy =>
//    {
//        policy.WithOrigins("http://localhost:5173", "https://reamp-brady-ltudsfaa7-depeng-zhangs-projects.vercel.app")
//              .AllowAnyHeader()
//              .AllowAnyMethod()
//              .AllowCredentials();
//    });
//});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", builder.Configuration["FrontendUrl"])
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});



//BlobService
builder.Services.AddSingleton(cf =>
{
    var config = cf.GetRequiredService<IConfiguration>();
    var connectionString = config["AzureBlobStorage:ConnectionString"];
    return new BlobServiceClient(connectionString);
});

// Services & Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IListingCaseRepository, ListingCaseRepository>();
builder.Services.AddScoped<IListingCaseService, ListingCaseService>();

builder.Services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();
builder.Services.AddScoped<IMediaAssetService, MediaAssetService>();

builder.Services.AddScoped<IStatusHistoryRepository, StatusHistoryRepository>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICaseContactRepository, CaseContactRepository>();
builder.Services.AddScoped<ICaseContactService, CaseContactService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IPhotographyCompanyRepository, PhotographyCompanyRepository>();
builder.Services.AddScoped<IPhotographyCompanyService, PhotographyCompanyService>();

builder.Services.AddScoped<ITokenService, JwtService>();

builder.Services.AddSingleton<ILoggerService, MongoLoggerService>();

builder.Services.AddScoped<ExceptionHandlingService>();

builder.Services.AddScoped<IEmailAdvancedSender, SmtpEmailSender>();

builder.Services.AddSingleton<IAzureBlobStorageService, AzureBlobStorageService>();

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(Program));

// MVC / JSON / Validation / AutoMapper
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    })
    .AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<Program>();
    });


builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Middleware pipeline
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseRouting();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionService = context.RequestServices.GetRequiredService<ExceptionHandlingService>();
        await exceptionService.HandleExceptionAsync(context);
    });
});

JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

// Role seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Seeding roles...");
        await ApplicationDbInitializer.SeedRolesAsync(services);
        logger.LogInformation("Seeding roles completed.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the roles.");
    }
}

app.Run();
